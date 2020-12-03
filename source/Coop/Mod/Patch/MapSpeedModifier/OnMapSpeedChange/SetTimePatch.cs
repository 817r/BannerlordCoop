using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace Coop.Mod.Patch.MapSpeedModifier.OnMapSpeedChange
{

    /// <summary>
    ///     <para>
    ///         Creates a flag when <c>Campaign::SetTimeSpeed</c> gets called.
    ///     </para>
    ///     <para>
    ///         More about it on <see href="https://github.com/Bannerlord-Coop-Team/BannerlordCoop/issues/113">issue #113</see>
    ///         and check <see href="https://github.com/Bannerlord-Coop-Team/BannerlordCoop/issues/108">issue #108</see> for more information.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     This class prefixes <c>Campaign::SetTimeSpeed</c> to add a flag to know
    ///     when the user have changed the map speed.
    /// </remarks>
    [HarmonyPatch(typeof(Campaign), nameof(Campaign.SetTimeSpeed))]
    class SetTimePatch
    {
        
        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Normal)]
        static void Prefix()
        {

            TimeControl.Instance.CanChangeSpeedControl = true;

        }

    }
}
