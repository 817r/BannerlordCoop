using System;
using System.Linq;
using Coop.Mod.Persistence;
using HarmonyLib;
using Mono.Reflection;
using Sync;
using TaleWorlds.CampaignSystem;

namespace Coop.Mod.Patch
{
    public sealed class TimeControl
    {
        public static TimeControl Instance { get; } = new TimeControl();

        static TimeControl()
        {
        }

        private TimeControl()
        {
        }

        private readonly PropertyPatch TimeControlPatch =
            new PropertyPatch(typeof(Campaign)).InterceptSetter(
                nameof(Campaign.TimeControlMode));

        private readonly PropertyPatch TimeControlLockPatch =
            new PropertyPatch(typeof(Campaign)).InterceptSetter(
                nameof(Campaign.TimeControlModeLock));
        /*
        private readonly PropertyPatch IsMainPartyWaitingPatch =
            new PropertyPatch(typeof(Campaign), EPatchBehaviour.NeverCallOriginal).InterceptSetter(
                nameof(Campaign.IsMainPartyWaiting));*/

        public FieldAccess<Campaign, CampaignTimeControlMode> TimeControlMode { get; } =
            new FieldAccess<Campaign, CampaignTimeControlMode>(
                AccessTools.Property(typeof(Campaign), nameof(Campaign.TimeControlMode))
                .GetBackingField());

        public FieldAccess<Campaign, bool> TimeControlModeLock { get; } =
            new FieldAccess<Campaign, bool>(
                AccessTools.Property(typeof(Campaign), nameof(Campaign.TimeControlModeLock))
                .GetBackingField());

        /// <summary>
        /// Determine if the user has changed the map speed using input keys.
        /// </summary>
        public bool CanChangeSpeedControl = false;

        [PatchInitializer]
        public static void Init()
        {
            FieldChangeBuffer.Intercept(Instance.TimeControlMode, Instance.TimeControlPatch.Setters, Instance.DoChangeTimeControl);
            FieldChangeBuffer.Intercept(Instance.TimeControlModeLock, Instance.TimeControlLockPatch.Setters, Instance.DoChangeTimeControl);

            /*MethodAccess mainPartyWaitingSetter = Instance.IsMainPartyWaitingPatch.Setters.First();
            mainPartyWaitingSetter.Condition = o => Coop.DoSync();
            mainPartyWaitingSetter.SetGlobalHandler(SetIsMainPartyWaiting);*/
        }

        /*private static void SetIsMainPartyWaiting(object instance, object value)
        {
            IEnvironmentClient env = CoopClient.Instance?.Persistence?.Environment;
            if (env == null) return;
            if (!(value is object[] args)) throw new ArgumentException();
            if (!(args[0] is bool isLocalMainPartyWaiting)) throw new ArgumentException();
            if (!(instance is Campaign campaign)) throw new ArgumentException();

            bool isEveryMainPartyWaiting = isLocalMainPartyWaiting;
            foreach (MobileParty party in env.PlayerControlledParties)
            {
                isEveryMainPartyWaiting = isEveryMainPartyWaiting && party.ComputeIsWaiting();
            }

            Instance.IsMainPartyWaitingPatch
                .Setters.First()
                .CallOriginal(instance, new object[] { isEveryMainPartyWaiting });
        }*/

        public bool DoChangeTimeControl()
        {
            return ShouldSetTimeControl() && !GetTimeControlModeLock() && CoopCampaign.Instance.IsCampaignActive && Coop.DoSync();
        }

        public bool ShouldSetTimeControl()
        {
            return CanChangeSpeedControl;
        }

        public CampaignTimeControlMode GetTimeControlMode()
        {
            return CoopCampaign.Instance.IsCampaignActive ? TimeControlMode.GetTyped(CoopCampaign.Instance.CurrentCampaign) : CampaignTimeControlMode.Stop;
        }

        public void SetTimeControlMode(CampaignTimeControlMode value)
        {
            if ( DoChangeTimeControl() && value != GetTimeControlMode() )
            {
                TimeControlMode.SetTyped(CoopCampaign.Instance.CurrentCampaign, value);
                CanChangeSpeedControl = false;
            }
        }

        public bool GetTimeControlModeLock()
        {
            return CoopCampaign.Instance.IsCampaignActive && TimeControlModeLock.GetTyped(CoopCampaign.Instance.CurrentCampaign);
        }

        public void SetTimeControlModeLock(bool value)
        {
            if (CoopCampaign.Instance.IsCampaignActive)
            {
                TimeControlModeLock.Set(CoopCampaign.Instance.CurrentCampaign, value);
            }
        }

        public void Lock()
        {
            this.SetTimeControlModeLock(true);
        }

        public void Unlock()
        {
            this.SetTimeControlModeLock(false);
        }

        public void LockStopped()
        {
            this.SetTimeControlMode(CampaignTimeControlMode.Stop);
            this.Lock();
        }
    }
}
