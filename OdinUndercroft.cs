using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using OdinUndercroft.Patches;
using PieceManager;
using ServerSync;
using UnityEngine;
using LocalizationManager;

namespace OdinUndercroft
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class OdinUndercroftPlugin : BaseUnityPlugin
    {
        public const string ModVersion = "1.3.2";
        public const string ModName = "OdinsUndercroft";
        internal const string Author = "Gravebear";
        private const string ModGUID = "gravebear.odinsundercroft";

        private static string _configFileName = ModGUID + ".cfg";
        private static string _configFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + _configFileName;
        public static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource OdinUndercroftPluginLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        internal static ConfigEntry<int> MaxNestedLimit = null!;
        internal static ConfigEntry<bool> EnableRemovalCheck = null!;
        internal static ConfigEntry<bool> EnableOUHall = null!;
        internal static ConfigEntry<bool> EnableOURoomA = null!;
        internal static ConfigEntry<bool> EnableOURoomB = null!;

        /// <summary>
        /// The registered Undercroft prefab, cached so config reloads can re-apply layer toggles
        /// to the prefab itself and not just to already-placed instances.
        /// </summary>
        private static GameObject? _undercroftPrefab;

        private void Awake()
        {
            Localizer.Load();

            // Bind configs
            MaxNestedLimit = config("General", "Max nested basements", 1,
                "The maximum number of basements you can incept into each other, changing this setting may cause dungeon collision.");
            EnableRemovalCheck = config("General", "Enable Removal Check", true,
                "Enable check to prevent removal of basements with buildings inside.");
            EnableOUHall = config("General", "Enable OU_Hall", true, "Enable Hall layer on Undercroft");
            EnableOURoomA = config("General", "Enable OU_Room_A", true, "Enable Room A layer on Undercroft");
            EnableOURoomB = config("General", "Enable OU_Room_B", true, "Enable Room B layer on Undercroft");

            // Patch first. If anything below throws, the patches are already applied rather than
            // leaving the game with half-registered pieces and zero Harmony patches.
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                _harmony.PatchAll(assembly);
            }
            catch (Exception ex)
            {
                OdinUndercroftPluginLogger.LogError($"Failed to apply Harmony patches: {ex}");
            }

            try
            {
                Functions.RegisterAllSFX();
            }
            catch (Exception ex)
            {
                OdinUndercroftPluginLogger.LogError($"Failed to register SFX prefabs: {ex}");
            }

            RegisterPieces();

            SetupWatcher();
        }

        private void RegisterPieces()
        {
            BuildPiece? odinsUndercroft = RegisterPiece("OdinsUndercroft", "Stone", 200, isBasement: true);
            _undercroftPrefab = odinsUndercroft?.Prefab;

            RegisterPiece("OdinsMinicroft", "Stone", 50, isBasement: true);

            RegisterPiece("OU_MetalGrate", "Iron", 1);
            RegisterPiece("OU_Urn", "Stone", 6);
            RegisterPiece("OU_Sarcophagus", "Stone", 10);
            RegisterPiece("OU_Sarcophagus_Lid", "Stone", 3);

            RegisterPiece("OU_Skeleton_Full", "BoneFragments", 4);
            RegisterPiece("OU_Skeleton_Ribs", "BoneFragments", 2);
            RegisterPiece("OU_Skeleton_Hanging", "BoneFragments", 2);
            RegisterPiece("OU_Skeleton_Pile", "BoneFragments", 2);

            RegisterPiece("OU_StoneArchway", "Stone", 2);
            RegisterPiece("OU_StoneWall", "Stone", 2);
            RegisterPiece("OU_StoneHalfWall", "Stone", 1);

            RegisterPiece("OU_Stone_Floor_1x1", "Stone", 1);
            RegisterPiece("OU_Stone_Floor_2x1", "Stone", 2);
            RegisterPiece("OU_Stone_Floor_2x2", "Stone", 2);

            RegisterPiece("OU_Stone_Roof_45", "Stone", 2, shaderSwap: true);
            RegisterPiece("OU_Stone_Outter_Corner", "Stone", 2, shaderSwap: true);
            RegisterPiece("OU_Stone_Roof_Corner", "Stone", 2, shaderSwap: true);

            RegisterPiece("OU_DrainPipe", "Stone", 2);
            RegisterPiece("OU_CornerCap", "Stone", 2);
            RegisterPiece("OU_CornerCap_Small", "Stone", 1);
            RegisterPiece("OU_StoneBeam", "Stone", 2);
            RegisterPiece("OU_StoneBeam_Small", "Stone", 1);

            RegisterPiece("OU_Iron_Cage", "Iron", 4);
            RegisterPiece("OU_Metal_Cage", "BlackMetal", 4);

            RegisterPiece("OU_Swords_Crossed", "Stone", 2);
            RegisterPiece("OU_Wall_Shield", "Stone", 2);

            RegisterPiece("OU_StoneRoof_Tile", "Stone", 2, shaderSwap: true);
            RegisterPiece("OU_StoneFloor", "Stone", 2);
            RegisterPiece("OU_StoneStair", "Stone", 6);

            RegisterPiece("OU_Large_Stone_Pillar", "Stone", 10);
            RegisterPiece("OU_Medium_Stone_Pillar", "Stone", 6);

            RegisterPiece("OH_Undercroft_BuildSkull", "BoneFragments", 1);

            if (_undercroftPrefab != null)
            {
                ToggleLayers(_undercroftPrefab);
            }
        }

        /// <summary>
        /// Registers a single build piece. A failure on one prefab (missing asset, bundle mismatch,
        /// missing Basement type) is logged and skipped instead of aborting the whole registration.
        /// </summary>
        private static BuildPiece? RegisterPiece(string prefabName, string requiredItem, int amount,
            bool shaderSwap = false, bool isBasement = false)
        {
            try
            {
                BuildPiece piece = new("odins_undercroft", prefabName);
                piece.Category.Set("Undercroft");
                piece.RequiredItems.Add(requiredItem, amount, true);

                if (shaderSwap)
                {
                    MaterialReplacer.RegisterGameObjectForShaderSwap(piece.Prefab,
                        MaterialReplacer.ShaderType.UseUnityShader);
                }

                if (isBasement)
                {
                    if (piece.Prefab == null)
                    {
                        OdinUndercroftPluginLogger.LogError(
                            $"Prefab '{prefabName}' resolved to null; cannot attach Basement component.");
                        return piece;
                    }

                    piece.Prefab.gameObject.AddComponent<Basement>();
                }

                return piece;
            }
            catch (Exception ex)
            {
                OdinUndercroftPluginLogger.LogError($"Failed to register piece '{prefabName}': {ex}");
                return null;
            }
        }

        private static void ToggleLayers(GameObject prefab)
        {
            if (prefab == null) return;

            Transform interior = prefab.transform.Find("interior/enabled");
            if (interior == null) return;

            Transform OU_Hall = interior.Find("OU_Hall");
            if (OU_Hall != null) OU_Hall.gameObject.SetActive(EnableOUHall.Value);

            Transform OU_Room_A = interior.Find("OU_Room_A");
            if (OU_Room_A != null) OU_Room_A.gameObject.SetActive(EnableOURoomA.Value);

            Transform OU_Room_B = interior.Find("OU_Room_B");
            if (OU_Room_B != null) OU_Room_B.gameObject.SetActive(EnableOURoomB.Value);
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, _configFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(_configFileFullPath)) return;
            try
            {
                OdinUndercroftPluginLogger.LogDebug("ReadConfigValues called");
                Config.Reload();

                // Re-apply layer toggles to the prefab itself...
                if (_undercroftPrefab != null)
                {
                    ToggleLayers(_undercroftPrefab);
                }

                // ...and to anything already placed in the world.
                foreach (Basement instance in FindObjectsOfType<Basement>())
                {
                    if (instance != null)
                    {
                        ToggleLayers(instance.gameObject);
                    }
                }
            }
            catch (Exception ex)
            {
                OdinUndercroftPluginLogger.LogError($"There was an issue loading your {_configFileName}: {ex}");
                OdinUndercroftPluginLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        #region ServerSync Stuff

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        internal static void TextAreaDrawer(ConfigEntryBase entry)
        {
            GUILayout.ExpandHeight(true);
            GUILayout.ExpandWidth(true);
            entry.BoxedValue = GUILayout.TextArea((string)entry.BoxedValue, GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));
        }

        private static readonly ConfigSync ConfigSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        #endregion
    }
}