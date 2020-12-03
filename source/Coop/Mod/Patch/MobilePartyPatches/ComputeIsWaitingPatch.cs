using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace Coop.Mod.Patch.MobilePartyPatches
{
    [HarmonyPatch(typeof(Campaign),nameof(Campaign.IsMainPartyWaiting), MethodType.Getter)]
    class ComputeIsWaitingPatch
    {
        private static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
