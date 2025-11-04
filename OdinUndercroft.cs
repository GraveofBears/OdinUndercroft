using System;
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
        public const string ModVersion = "1.2.9";
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

        private void Awake()
        {
            Localizer.Load();
            // Bind configs
            MaxNestedLimit = config("General", "Max nested basements", 1,
                "The maximum number of basements you can incept into each other, changing this setting may cause dungeon collision.");
            EnableRemovalCheck = config("General", "Enable Removal Check", true, "Enable check to prevent removal of basements with buildings inside."); // Bind new config
            EnableOUHall = config("General", "Enable OU_Hall", true, "Enable Hall layer on Undercroft");
            EnableOURoomA = config("General", "Enable OU_Room_A", true, "Enable Room A layer on Undercroft");
            EnableOURoomB = config("General", "Enable OU_Room_B", true, "Enable Room B layer on Undercroft");

            BuildPiece OdinsUndercroft = new("odins_undercroft", "OdinsUndercroft");
            OdinsUndercroft.Category.Set("Undercroft");
            OdinsUndercroft.RequiredItems.Add("Stone", 200, true);
            OdinsUndercroft.Prefab.gameObject.AddComponent<Basement>();

            BuildPiece OdinsMinicroft = new("odins_undercroft", "OdinsMinicroft");
            OdinsMinicroft.Category.Set("Undercroft");
            OdinsMinicroft.RequiredItems.Add("Stone", 50, true);
            OdinsMinicroft.Prefab.gameObject.AddComponent<Basement>();

            BuildPiece OU_MetalGrate = new("odins_undercroft", "OU_MetalGrate");
            OU_MetalGrate.Category.Set("Undercroft");
            OU_MetalGrate.RequiredItems.Add("Iron", 1, true);

            BuildPiece OU_Urn = new("odins_undercroft", "OU_Urn");
            OU_Urn.Category.Set("Undercroft");
            OU_Urn.RequiredItems.Add("Stone", 6, true);

            BuildPiece OU_Sarcophagus = new("odins_undercroft", "OU_Sarcophagus");
            OU_Sarcophagus.Category.Set("Undercroft");
            OU_Sarcophagus.RequiredItems.Add("Stone", 10, true);

            BuildPiece OU_Sarcophagus_Lid = new("odins_undercroft", "OU_Sarcophagus_Lid");
            OU_Sarcophagus_Lid.Category.Set("Undercroft");
            OU_Sarcophagus_Lid.RequiredItems.Add("Stone", 3, true);

            BuildPiece OU_Skeleton_Full = new("odins_undercroft", "OU_Skeleton_Full");
            OU_Skeleton_Full.Category.Set("Undercroft");
            OU_Skeleton_Full.RequiredItems.Add("BoneFragments", 4, true);

            BuildPiece OU_Skeleton_Ribs = new("odins_undercroft", "OU_Skeleton_Ribs");
            OU_Skeleton_Ribs.Category.Set("Undercroft");
            OU_Skeleton_Ribs.RequiredItems.Add("BoneFragments", 2, true);

            BuildPiece OU_Skeleton_Hanging = new("odins_undercroft", "OU_Skeleton_Hanging");
            OU_Skeleton_Hanging.Category.Set("Undercroft");
            OU_Skeleton_Hanging.RequiredItems.Add("BoneFragments", 2, true);

            BuildPiece OU_Skeleton_Pile = new("odins_undercroft", "OU_Skeleton_Pile");
            OU_Skeleton_Pile.Category.Set("Undercroft");
            OU_Skeleton_Pile.RequiredItems.Add("BoneFragments", 2, true);

            BuildPiece OU_StoneArchway = new("odins_undercroft", "OU_StoneArchway");
            OU_StoneArchway.Category.Set("Undercroft");
            OU_StoneArchway.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_StoneWall = new("odins_undercroft", "OU_StoneWall");
            OU_StoneWall.Category.Set("Undercroft");
            OU_StoneWall.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_StoneHalfWall = new("odins_undercroft", "OU_StoneHalfWall");
            OU_StoneHalfWall.Category.Set("Undercroft");
            OU_StoneHalfWall.RequiredItems.Add("Stone", 1, true);

            BuildPiece OU_Stone_Floor_1x1 = new("odins_undercroft", "OU_Stone_Floor_1x1");
            OU_Stone_Floor_1x1.Category.Set("Undercroft");
            OU_Stone_Floor_1x1.RequiredItems.Add("Stone", 1, true);

            BuildPiece OU_Stone_Floor_2x1 = new("odins_undercroft", "OU_Stone_Floor_2x1");
            OU_Stone_Floor_2x1.Category.Set("Undercroft");
            OU_Stone_Floor_2x1.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_Stone_Floor_2x2 = new("odins_undercroft", "OU_Stone_Floor_2x2");
            OU_Stone_Floor_2x2.Category.Set("Undercroft");
            OU_Stone_Floor_2x2.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_Stone_Roof_45 = new("odins_undercroft", "OU_Stone_Roof_45");
            OU_Stone_Roof_45.Category.Set("Undercroft");
            OU_Stone_Roof_45.RequiredItems.Add("Stone", 2, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(OU_Stone_Roof_45.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece OU_Stone_Outter_Corner = new("odins_undercroft", "OU_Stone_Outter_Corner");
            OU_Stone_Outter_Corner.Category.Set("Undercroft");
            OU_Stone_Outter_Corner.RequiredItems.Add("Stone", 2, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(OU_Stone_Outter_Corner.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece OU_Stone_Roof_Corner = new("odins_undercroft", "OU_Stone_Roof_Corner");
            OU_Stone_Roof_Corner.Category.Set("Undercroft");
            OU_Stone_Roof_Corner.RequiredItems.Add("Stone", 2, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(OU_Stone_Roof_Corner.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece OU_DrainPipe = new("odins_undercroft", "OU_DrainPipe");
            OU_DrainPipe.Category.Set("Undercroft");
            OU_DrainPipe.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_CornerCap = new("odins_undercroft", "OU_CornerCap");
            OU_CornerCap.Category.Set("Undercroft");
            OU_CornerCap.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_CornerCap_Small = new("odins_undercroft", "OU_CornerCap_Small");
            OU_CornerCap_Small.Category.Set("Undercroft");
            OU_CornerCap_Small.RequiredItems.Add("Stone", 1, true);

            BuildPiece OU_StoneBeam = new("odins_undercroft", "OU_StoneBeam");
            OU_StoneBeam.Category.Set("Undercroft");
            OU_StoneBeam.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_StoneBeam_Small = new("odins_undercroft", "OU_StoneBeam_Small");
            OU_StoneBeam_Small.Category.Set("Undercroft");
            OU_StoneBeam_Small.RequiredItems.Add("Stone", 1, true);

            BuildPiece OU_Iron_Cage = new("odins_undercroft", "OU_Iron_Cage");
            OU_Iron_Cage.Category.Set("Undercroft");
            OU_Iron_Cage.RequiredItems.Add("Iron", 4, true);

            BuildPiece OU_Metal_Cage = new("odins_undercroft", "OU_Metal_Cage");
            OU_Metal_Cage.Category.Set("Undercroft");
            OU_Metal_Cage.RequiredItems.Add("BlackMetal", 4, true);

            BuildPiece OU_Swords_Crossed = new("odins_undercroft", "OU_Swords_Crossed");
            OU_Swords_Crossed.Category.Set("Undercroft");
            OU_Swords_Crossed.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_Wall_Shield = new("odins_undercroft", "OU_Wall_Shield");
            OU_Wall_Shield.Category.Set("Undercroft");
            OU_Wall_Shield.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_StoneRoof_Tile = new("odins_undercroft", "OU_StoneRoof_Tile");
            OU_StoneRoof_Tile.Category.Set("Undercroft");
            OU_StoneRoof_Tile.RequiredItems.Add("Stone", 2, true);
            MaterialReplacer.RegisterGameObjectForShaderSwap(OU_StoneRoof_Tile.Prefab, MaterialReplacer.ShaderType.UseUnityShader);

            BuildPiece OU_StoneFloor = new("odins_undercroft", "OU_StoneFloor");
            OU_StoneFloor.Category.Set("Undercroft");
            OU_StoneFloor.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_StoneStair = new("odins_undercroft", "OU_StoneStair");
            OU_StoneStair.Category.Set("Undercroft");
            OU_StoneStair.RequiredItems.Add("Stone", 6, true);

            BuildPiece OU_Large_Stone_Pillar = new("odins_undercroft", "OU_Large_Stone_Pillar");
            OU_Large_Stone_Pillar.Category.Set("Undercroft");
            OU_Large_Stone_Pillar.RequiredItems.Add("Stone", 10, true);

            BuildPiece OU_Medium_Stone_Pillar = new("odins_undercroft", "OU_Medium_Stone_Pillar");
            OU_Medium_Stone_Pillar.Category.Set("Undercroft");
            OU_Medium_Stone_Pillar.RequiredItems.Add("Stone", 6, true);

            BuildPiece OH_Undercroft_BuildSkull = new("odins_undercroft", "OH_Undercroft_BuildSkull");
            OH_Undercroft_BuildSkull.Category.Set("Undercroft");
            OH_Undercroft_BuildSkull.RequiredItems.Add("BoneFragments", 1, true);

            ToggleLayers(OdinsUndercroft.Prefab);

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void ToggleLayers(GameObject prefab)
        {
            Transform interior = prefab.transform.Find("interior/enabled");
            if (interior != null)
            {
                Transform OU_Hall = interior.Find("OU_Hall");
                if (OU_Hall != null) OU_Hall.gameObject.SetActive(EnableOUHall.Value);

                Transform OU_Room_A = interior.Find("OU_Room_A");
                if (OU_Room_A != null) OU_Room_A.gameObject.SetActive(EnableOURoomA.Value);

                Transform OU_Room_B = interior.Find("OU_Room_B");
                if (OU_Room_B != null) OU_Room_B.gameObject.SetActive(EnableOURoomB.Value);
            }
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

                // Loop through all instances of the OdinsUndercroft prefab and apply layer toggles
                foreach (var instance in FindObjectsOfType<Basement>())
                {
                    ToggleLayers(instance.gameObject);
                }
            }
            catch
            {
                OdinUndercroftPluginLogger.LogError($"There was an issue loading your {_configFileName}");
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
