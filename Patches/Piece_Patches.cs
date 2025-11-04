using HarmonyLib;
using PieceManager;
using UnityEngine;

namespace OdinUndercroft.Patches
{
    [HarmonyPatch(typeof(Piece), nameof(Piece.CanBeRemoved))]
    static class Piece_Patches
    {
        static void Postfix(Piece __instance, ref bool __result)
        {
            // Check if the removal check is enabled
            if (!OdinUndercroft.OdinUndercroftPlugin.EnableRemovalCheck.Value)
                return;

            var basement = __instance.GetComponent<Basement>();
            if (basement)
            {
                if (!basement.CanBeRemoved())
                    __result = false;
            }
        }
    }
}
