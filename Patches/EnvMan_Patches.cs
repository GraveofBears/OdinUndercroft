using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace OdinUndercroft.Patches
{
    /// <summary>
    /// Clones the vanilla "Crypt" environment into a lightless, silent, fogless "Basement"
    /// environment used inside the Undercroft.
    ///
    /// Named distinctly (rather than the generic EnvMan_Patches) because rolopogo's Basements
    /// mod patches the same method. Harmony dispatches on the target method, not the class name,
    /// so both postfixes still run -- the guard below is what actually prevents a duplicate
    /// registration. The distinct name just makes stack traces and Harmony patch dumps readable.
    /// </summary>
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Awake))]
    static class OdinUndercroft_EnvMan_Awake_Patch
    {
        static void Postfix(EnvMan __instance)
        {
            if (__instance == null || __instance.m_environments == null) return;

            // Already registered -- either by us on a previous EnvMan.Awake (main-menu round trip)
            // or by another mod that provides the same environment.
            if (__instance.m_environments.Any(x => x != null && x.m_name == Functions.BasementEnvName))
            {
                return;
            }

            EnvSetup source = __instance.m_environments.Find(x => x != null && x.m_name == Functions.SourceEnvName);
            if (source == null)
            {
                OdinUndercroftPlugin.OdinUndercroftPluginLogger.LogError(
                    $"Environment '{Functions.SourceEnvName}' not found; '{Functions.BasementEnvName}' will not be registered. " +
                    "Another mod may have renamed or replaced it.");
                return;
            }

            EnvSetup basementEnv;
            try
            {
                basementEnv = source.Clone();
            }
            catch (Exception ex)
            {
                OdinUndercroftPlugin.OdinUndercroftPluginLogger.LogError(
                    $"Failed to clone '{Functions.SourceEnvName}' environment: {ex}");
                return;
            }

            basementEnv.m_name = Functions.BasementEnvName;
            basementEnv.m_ambientVol = 0;
            basementEnv.m_ambientList = "";
            basementEnv.m_ambientLoop = null;
            basementEnv.m_windMax = 0;
            basementEnv.m_windMin = 0;
            basementEnv.m_musicMorning = "";
            basementEnv.m_musicDay = "";
            basementEnv.m_musicEvening = "";
            basementEnv.m_musicNight = "";
            basementEnv.m_rainCloudAlpha = 0;
            basementEnv.m_fogDensityDay = 0;
            basementEnv.m_fogDensityEvening = 0;
            basementEnv.m_fogDensityMorning = 0;
            basementEnv.m_fogDensityNight = 0;
            basementEnv.m_fogColorDay = Color.clear;
            basementEnv.m_fogColorEvening = Color.clear;
            basementEnv.m_fogColorMorning = Color.clear;
            basementEnv.m_fogColorNight = Color.clear;
            basementEnv.m_fogColorSunDay = Color.clear;
            basementEnv.m_fogColorSunEvening = Color.clear;
            basementEnv.m_fogColorSunMorning = Color.clear;
            basementEnv.m_fogColorSunNight = Color.clear;
            basementEnv.m_psystems = Array.Empty<GameObject>();

            __instance.m_environments.Add(basementEnv);

            // Vanilla's own permission list for building inside an interior. The Character.InInterior
            // postfix is the other half of this, not a substitute for it. Without this entry,
            // placement inside the Undercroft relies on some other mod having registered the same
            // environment name.
            if (__instance.m_interiorBuildingOverrideEnvironments != null &&
                !__instance.m_interiorBuildingOverrideEnvironments.Contains(Functions.BasementEnvName))
            {
                __instance.m_interiorBuildingOverrideEnvironments.Add(Functions.BasementEnvName);
            }
        }
    }
}