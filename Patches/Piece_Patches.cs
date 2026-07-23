using HarmonyLib;
using PieceManager;
using UnityEngine;

namespace OdinUndercroft.Patches
{
    /// <summary>
    /// Blocks removal of a basement piece while it still contains player-built structures.
    /// </summary>
    [HarmonyPatch(typeof(Piece), nameof(Piece.CanBeRemoved))]
    static class OdinUndercroft_Piece_CanBeRemoved_Patch
    {
        static void Postfix(Piece __instance, ref bool __result)
        {
            // Nothing to do if it's already blocked.
            if (!__result) return;

            if (__instance == null) return;

            // Config can be null if Awake failed before the binds completed.
            var removalCheck = OdinUndercroftPlugin.EnableRemovalCheck;
            if (removalCheck == null || !removalCheck.Value) return;

            Basement basement = __instance.GetComponent<Basement>();
            if (basement == null) return;

            if (!basement.CanBeRemoved())
            {
                __result = false;
            }
        }
    }
}
