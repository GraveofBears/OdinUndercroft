using System;
using HarmonyLib;
using UnityEngine;

namespace OdinUndercroft.Patches
{
    /// <summary>
    /// The Undercroft is technically "interior", but reporting it as such breaks weather, lighting
    /// and building rules inside it. Force InInterior to false for the local player while they are
    /// in the basement environment.
    ///
    /// This runs very frequently, so every dereference is guarded -- EnvMan and the current
    /// environment can both be null during scene transitions.
    /// </summary>
    [HarmonyPatch(typeof(Character), nameof(Character.InInterior), typeof(Transform))]
    static class OdinUndercroft_Character_InInterior_Patch
    {
        static void Postfix(Character __instance, ref bool __result)
        {
            if (__instance == null) return;

            Player localPlayer = Player.m_localPlayer;
            if (localPlayer == null || __instance != localPlayer) return;

            if (Functions.IsInBasementEnvironment())
            {
                __result = false;
            }
        }
    }
}
