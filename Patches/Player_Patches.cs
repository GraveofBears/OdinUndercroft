using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace OdinUndercroft.Patches
{
    /// <summary>
    /// Two jobs:
    ///  1. Postfix -- forbid placing a basement that would overlap another basement, or that would
    ///     exceed the configured nesting depth.
    ///  2. Transpiler -- inside the basement environment there is no Heightmap, so the vanilla
    ///     "no ground here" checks for m_groundPiece / m_groundOnly must be neutralised.
    ///
    /// Named distinctly because rolopogo's Basements mod may patch the same method. If it also
    /// transpiles these two branch points, both delegates get chained -- check Basements.dll in
    /// ILSpy before assuming this is safe to ship alongside his version.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
    static class OdinUndercroft_Player_UpdatePlacementGhost_Patch
    {
        const float overlapRadius = 60;

        // Reflection resolved once, at type-init, instead of on every frame of ghost placement.
        static readonly Type? PlacementStatusType =
            typeof(Player).Assembly.GetType("Player+PlacementStatus");

        static readonly FieldInfo? MoreSpaceField =
            PlacementStatusType?.GetField("MoreSpace", BindingFlags.Public | BindingFlags.Static);

        static readonly object? MoreSpaceValue = MoreSpaceField?.GetValue(null);

        static readonly FieldInfo? PlacementStatusField =
            AccessTools.Field(typeof(Player), "m_placementStatus");

        static bool _reflectionFailureLogged;

        static bool ReflectionReady()
        {
            if (MoreSpaceValue != null && PlacementStatusField != null) return true;

            if (!_reflectionFailureLogged)
            {
                _reflectionFailureLogged = true;
                OdinUndercroftPlugin.OdinUndercroftPluginLogger.LogError(
                    "Could not resolve Player+PlacementStatus.MoreSpace or Player.m_placementStatus. " +
                    "Basement overlap checks are disabled. This usually means a Valheim update renamed them.");
            }

            return false;
        }

        static void Postfix(Player __instance, GameObject ___m_placementGhost)
        {
            if (__instance == null || ___m_placementGhost == null) return;

            Basement basementComponent = ___m_placementGhost.GetComponent<Basement>();
            if (basementComponent == null) return;

            if (Basement.allBasements == null || Basement.allBasements.Count <= 0) return;

            if (!ReflectionReady()) return;

            Vector3 ghostPosition = ___m_placementGhost.transform.position;

            IEnumerable<Basement> overlapping = Basement.allBasements
                .Where(x => x != null)
                .Where(x => x.gameObject != ___m_placementGhost)
                .Where(x => Vector3.Distance(x.transform.position, ghostPosition) < overlapRadius);

            // NOTE: GetComponentInParent<Basement>() on a Basement always returns itself, so this
            // predicate is effectively "any nearby basement at all". Preserved from the original
            // behaviour -- if you meant "any nearby basement that is nested inside another", this
            // is the line to revisit.
            bool overlaps = overlapping.Any(x => x.GetComponentInParent<Basement>());

            float ceiling = 2500f * Mathf.Max(OdinUndercroftPlugin.MaxNestedLimit.Value, 0) + 2000f;
            bool tooDeep = ghostPosition.y > ceiling;

            if (overlaps || tooDeep)
            {
                PlacementStatusField!.SetValue(__instance, MoreSpaceValue);
            }
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdatePlacementGhostTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo objectEquality = AccessTools.Method(
                typeof(UnityEngine.Object), "op_Equality",
                new Type[] { typeof(UnityEngine.Object), typeof(UnityEngine.Object) });

            return new CodeMatcher(instructions)
                .MatchForward(
                    useEnd: false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Piece), nameof(Piece.m_groundPiece))),
                    new CodeMatch(OpCodes.Brfalse),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(OpCodes.Call, objectEquality))
                .ThrowIfNotMatch("Could not find the Piece.m_groundPiece heightmap null-check in Player.UpdatePlacementGhost")
                .Advance(offset: 5)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(HeightmapIsNullBasementDelegate))
                .MatchForward(
                    useEnd: false,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Piece), nameof(Piece.m_groundOnly))),
                    new CodeMatch(OpCodes.Brfalse),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldnull),
                    new CodeMatch(OpCodes.Call, objectEquality))
                .ThrowIfNotMatch("Could not find the Piece.m_groundOnly heightmap null-check in Player.UpdatePlacementGhost")
                .Advance(offset: 5)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(HeightmapIsNullBasementDelegate))
                .InstructionEnumeration();
        }

        /// <summary>
        /// Inside the basement there is no Heightmap, so "heightmap == null" must not mean
        /// "you cannot build here".
        /// </summary>
        static bool HeightmapIsNullBasementDelegate(bool isEqual)
        {
            return Functions.IsInBasementEnvironment() ? false : isEqual;
        }
    }
}
