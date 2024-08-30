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

namespace OdinUndercroft
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class OdinUndercroftPlugin : BaseUnityPlugin
    {
        public const string ModVersion = "1.1.21";
        public const string ModName = "OdinsUndercroft";
        internal const string Author = "Gravebear";
        private const string ModGUID = "gravebear.odinsundercroft";
        private static string _configFileName = ModGUID + ".cfg";
        private static string _configFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + _configFileName;
        public static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource OdinUndercroftPluginLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        internal static ConfigEntry<int> MaxNestedLimit = null!;

        private void Awake()
        {
            // Bind configs
            MaxNestedLimit = config("General", "Max nested basements", 1,
                "The maximum number of basements you can incept into each other");


            BuildPiece OdinsUndercroft = new("odins_undercroft", "OdinsUndercroft");
            OdinsUndercroft.Category.Set("Undercroft");
            OdinsUndercroft.Name.English("Odins Undercroft");
            OdinsUndercroft.Description.English("A large stone basement");
            OdinsUndercroft.RequiredItems.Add("Stone", 200, true);
            OdinsUndercroft.Prefab.gameObject.AddComponent<Basement>();

            BuildPiece OU_MetalGrate = new("odins_undercroft", "OU_MetalGrate");
            OU_MetalGrate.Category.Set("Undercroft");
            OU_MetalGrate.Name.English("Odins MetalGrate");
            OU_MetalGrate.Description.English("A large metal grate");
            OU_MetalGrate.RequiredItems.Add("Iron", 1, true);

            BuildPiece OU_Urn = new("odins_undercroft", "OU_Urn");
            OU_Urn.Category.Set("Undercroft");
            OU_Urn.Name.English("Odins Urn");
            OU_Urn.Description.English("A place to keep your remains");
            OU_Urn.RequiredItems.Add("Stone", 6, true);

            BuildPiece OU_Sarcophagus = new("odins_undercroft", "OU_Sarcophagus");
            OU_Sarcophagus.Category.Set("Undercroft");
            OU_Sarcophagus.Name.English("Odins Sarcophagus");
            OU_Sarcophagus.Description.English("A large stone Sarcophagus");
            OU_Sarcophagus.RequiredItems.Add("Stone", 10, true);

            BuildPiece OU_Sarcophagus_Lid = new("odins_undercroft", "OU_Sarcophagus_Lid");
            OU_Sarcophagus_Lid.Category.Set("Undercroft");
            OU_Sarcophagus_Lid.Name.English("Odins Sarcophagus Lid");
            OU_Sarcophagus_Lid.Description.English("A large stone Sarcophagus Lid");
            OU_Sarcophagus_Lid.RequiredItems.Add("Stone", 3, true);

            BuildPiece OU_Skeleton_Full = new("odins_undercroft", "OU_Skeleton_Full");
            OU_Skeleton_Full.Category.Set("Undercroft");
            OU_Skeleton_Full.Name.English("Odins Skeleton Full");
            OU_Skeleton_Full.Description.English("A Skeleton Full");
            OU_Skeleton_Full.RequiredItems.Add("BoneFragments", 4, true);

            BuildPiece OU_Skeleton_Ribs = new("odins_undercroft", "OU_Skeleton_Ribs");
            OU_Skeleton_Ribs.Category.Set("Undercroft");
            OU_Skeleton_Ribs.Name.English("Odins Skeleton Ribs");
            OU_Skeleton_Ribs.Description.English("A ribcage from a skeleton");
            OU_Skeleton_Ribs.RequiredItems.Add("BoneFragments", 2, true);

            BuildPiece OU_Skeleton_Hanging = new("odins_undercroft", "OU_Skeleton_Hanging");
            OU_Skeleton_Hanging.Category.Set("Undercroft");
            OU_Skeleton_Hanging.Name.English("Odins Skeleton Hanging");
            OU_Skeleton_Hanging.Description.English("A Skeleton Hung");
            OU_Skeleton_Hanging.RequiredItems.Add("BoneFragments", 2, true);

            BuildPiece OU_Skeleton_Pile = new("odins_undercroft", "OU_Skeleton_Pile");
            OU_Skeleton_Pile.Category.Set("Undercroft");
            OU_Skeleton_Pile.Name.English("Odins Skeleton Pile");
            OU_Skeleton_Pile.Description.English("A Skeleton Pile");
            OU_Skeleton_Pile.RequiredItems.Add("BoneFragments", 2, true);

            BuildPiece OU_StoneArchway = new("odins_undercroft", "OU_StoneArchway");
            OU_StoneArchway.Category.Set("Undercroft");
            OU_StoneArchway.Name.English("Odins Stone Archway");
            OU_StoneArchway.Description.English("A stone Stone Archway");
            OU_StoneArchway.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_StoneWall = new("odins_undercroft", "OU_StoneWall");
            OU_StoneWall.Category.Set("Undercroft");
            OU_StoneWall.Name.English("Odins StoneWall");
            OU_StoneWall.Description.English("A stone wall");
            OU_StoneWall.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_StoneHalfWall = new("odins_undercroft", "OU_StoneHalfWall");
            OU_StoneHalfWall.Category.Set("Undercroft");
            OU_StoneHalfWall.Name.English("Odins StoneHalfWall");
            OU_StoneHalfWall.Description.English("A stone half wall");
            OU_StoneHalfWall.RequiredItems.Add("Stone", 1, true);

            BuildPiece OU_Stone_Floor_1x1 = new("odins_undercroft", "OU_Stone_Floor_1x1");
            OU_Stone_Floor_1x1.Category.Set("Undercroft");
            OU_Stone_Floor_1x1.Name.English("Odins Stone Floor 1x1");
            OU_Stone_Floor_1x1.Description.English("A stone 1x1 floor piece");
            OU_Stone_Floor_1x1.RequiredItems.Add("Stone", 1, true);

            BuildPiece OU_Stone_Floor_2x1 = new("odins_undercroft", "OU_Stone_Floor_2x1");
            OU_Stone_Floor_2x1.Category.Set("Undercroft");
            OU_Stone_Floor_2x1.Name.English("Odins Stone Floor 2x1");
            OU_Stone_Floor_2x1.Description.English("A stone 2x1 floor piece");
            OU_Stone_Floor_2x1.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_Stone_Floor_2x2 = new("odins_undercroft", "OU_Stone_Floor_2x2");
            OU_Stone_Floor_2x2.Category.Set("Undercroft");
            OU_Stone_Floor_2x2.Name.English("Odins Stone Floor 2x2");
            OU_Stone_Floor_2x2.Description.English("A stone 2x2 floor piece");
            OU_Stone_Floor_2x2.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_Stone_Roof_45 = new("odins_undercroft", "OU_Stone_Roof_45");
            OU_Stone_Roof_45.Category.Set("Undercroft");
            OU_Stone_Roof_45.Name.English("Odins Stone Roof 45");
            OU_Stone_Roof_45.Description.English("A stone 45 roof");
            OU_Stone_Roof_45.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_Stone_Outter_Corner = new("odins_undercroft", "OU_Stone_Outter_Corner");
            OU_Stone_Outter_Corner.Category.Set("Undercroft");
            OU_Stone_Outter_Corner.Name.English("Odins Stone Outside Corner");
            OU_Stone_Outter_Corner.Description.English("A stone roof outter corner");
            OU_Stone_Outter_Corner.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_Stone_Roof_Corner = new("odins_undercroft", "OU_Stone_Roof_Corner");
            OU_Stone_Roof_Corner.Category.Set("Undercroft");
            OU_Stone_Roof_Corner.Name.English("Odins Stone Roof Corner");
            OU_Stone_Roof_Corner.Description.English("A stone roof corner");
            OU_Stone_Roof_Corner.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_DrainPipe = new("odins_undercroft", "OU_DrainPipe");
            OU_DrainPipe.Category.Set("Undercroft");
            OU_DrainPipe.Name.English("Odins Drinpipe");
            OU_DrainPipe.Description.English("A stone drain deco");
            OU_DrainPipe.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_CornerCap = new("odins_undercroft", "OU_CornerCap");
            OU_CornerCap.Category.Set("Undercroft");
            OU_CornerCap.Name.English("Odins CornerCap");
            OU_CornerCap.Description.English("A stone corner cap for OU walls");
            OU_CornerCap.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_CornerCap_Small = new("odins_undercroft", "OU_CornerCap_Small");
            OU_CornerCap_Small.Category.Set("Undercroft");
            OU_CornerCap_Small.Name.English("Odins CornerCap Small");
            OU_CornerCap_Small.Description.English("A small stone corner cap for OU walls");
            OU_CornerCap_Small.RequiredItems.Add("Stone", 1, true);

            BuildPiece OU_StoneBeam = new("odins_undercroft", "OU_StoneBeam");
            OU_StoneBeam.Category.Set("Undercroft");
            OU_StoneBeam.Name.English("Odins Stone Beam");
            OU_StoneBeam.Description.English("A stone beam cap for OU walls");
            OU_StoneBeam.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_StoneBeam_Small = new("odins_undercroft", "OU_StoneBeam_Small");
            OU_StoneBeam_Small.Category.Set("Undercroft");
            OU_StoneBeam_Small.Name.English("Odins Stone Beam Small");
            OU_StoneBeam_Small.Description.English("A small stone beam cap for OU walls");
            OU_StoneBeam_Small.RequiredItems.Add("Stone", 1, true);

            BuildPiece OU_Iron_Cage = new("odins_undercroft", "OU_Iron_Cage");
            OU_Iron_Cage.Category.Set("Undercroft");
            OU_Iron_Cage.Name.English("Odins Iron Cage");
            OU_Iron_Cage.Description.English("An iron cage");
            OU_Iron_Cage.RequiredItems.Add("Iron", 4, true);

            BuildPiece OU_Metal_Cage = new("odins_undercroft", "OU_Metal_Cage");
            OU_Metal_Cage.Category.Set("Undercroft");
            OU_Metal_Cage.Name.English("Odins BlackMetal Cage");
            OU_Metal_Cage.Description.English("An blackmetal cage");
            OU_Metal_Cage.RequiredItems.Add("BlackMetal", 4, true);

            BuildPiece OU_Swords_Crossed = new("odins_undercroft", "OU_Swords_Crossed");
            OU_Swords_Crossed.Category.Set("Undercroft");
            OU_Swords_Crossed.Name.English("Odins Crossed Swords");
            OU_Swords_Crossed.Description.English("A stone pare of swords");
            OU_Swords_Crossed.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_Wall_Shield = new("odins_undercroft", "OU_Wall_Shield");
            OU_Wall_Shield.Category.Set("Undercroft");
            OU_Wall_Shield.Name.English("Odins Wall Shield");
            OU_Wall_Shield.Description.English("A shield deco for walls");
            OU_Wall_Shield.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_StoneRoof_Tile = new("odins_undercroft", "OU_StoneRoof_Tile");
            OU_StoneRoof_Tile.Category.Set("Undercroft");
            OU_StoneRoof_Tile.Name.English("Odins StoneRoof Tile");
            OU_StoneRoof_Tile.Description.English("A stone rooftile piece");
            OU_StoneRoof_Tile.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_StoneFloor = new("odins_undercroft", "OU_StoneFloor");
            OU_StoneFloor.Category.Set("Undercroft");
            OU_StoneFloor.Name.English("Odins Stone Floor");
            OU_StoneFloor.Description.English("A stone floor piece");
            OU_StoneFloor.RequiredItems.Add("Stone", 2, true);

            BuildPiece OU_StoneStair = new("odins_undercroft", "OU_StoneStair");
            OU_StoneStair.Category.Set("Undercroft");
            OU_StoneStair.Name.English("Odins StoneStairs");
            OU_StoneStair.Description.English("A stone stair piece");
            OU_StoneStair.RequiredItems.Add("Stone", 6, true);

            BuildPiece OU_Large_Stone_Pillar = new("odins_undercroft", "OU_Large_Stone_Pillar");
            OU_Large_Stone_Pillar.Category.Set("Undercroft");
            OU_Large_Stone_Pillar.Name.English("Odins Large Stone Pillar");
            OU_Large_Stone_Pillar.Description.English("A large stone pillar");
            OU_Large_Stone_Pillar.RequiredItems.Add("Stone", 10, true);

            BuildPiece OU_Medium_Stone_Pillar = new("odins_undercroft", "OU_Medium_Stone_Pillar");
            OU_Medium_Stone_Pillar.Category.Set("Undercroft");
            OU_Medium_Stone_Pillar.Name.English("Odins Medium Stone Pillar");
            OU_Medium_Stone_Pillar.Description.English("A medium stone pillar");
            OU_Medium_Stone_Pillar.RequiredItems.Add("Stone", 6, true);

            BuildPiece OH_Undercroft_BuildSkull = new("odins_undercroft", "OH_Undercroft_BuildSkull");
            OH_Undercroft_BuildSkull.Category.Set("Undercroft");
            OH_Undercroft_BuildSkull.Name.English("Odins Crafting Skull");
            OH_Undercroft_BuildSkull.Description.English("Sets Build Area for undercroft pieces.");
            OH_Undercroft_BuildSkull.RequiredItems.Add("BoneFragments", 1, true);

            //Functions.RegisterAllSFX();

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
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
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        /*
        internal ConfigEntry<T> TextEntryConfig<T>(string group, string name, T value, string desc,
            bool synchronizedSetting = true)
        {
            ConfigurationManagerAttributes attributes = new()
            {
                CustomDrawer = TextAreaDrawer
            };
            return config(group, name, value, new ConfigDescription(desc, null, attributes), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }
        */
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