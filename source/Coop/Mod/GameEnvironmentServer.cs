﻿using System;
using System.Linq;
using Coop.Mod.Patch;
using Coop.Mod.Persistence;
using Coop.Mod.Persistence.Party;
using Coop.Mod.Persistence.RPC;
using NLog;
using Sync;
using Sync.Store;
using TaleWorlds.CampaignSystem;

namespace Coop.Mod
{
    public class GameEnvironmentServer : IEnvironmentServer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public FieldAccessGroup<MobileParty, MovementData> TargetPosition =>
            CampaignMapMovement.Movement;

        public bool CanChangeTimeControlMode => CoopServer.Instance.AreAllClientsPlaying;

        public EventBroadcastingQueue EventQueue => CoopServer.Instance.Persistence?.EventQueue;

        public MobileParty GetMobilePartyByIndex(int iPartyIndex)
        {
            MobileParty ret = null;
            GameLoopRunner.RunOnMainThread(
                () =>
                {
                    ret = MobileParty.All.SingleOrDefault(p => p.Party.Index == iPartyIndex);
                });
            return ret;
        }

        public SharedRemoteStore Store =>
            CoopServer.Instance.SyncedObjectStore ??
            throw new InvalidOperationException("Client not initialized.");

        public void TimeControlStop()
        {
            Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
        }

        public void LockTimeControlStopped()
        {
            Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
            Campaign.Current.SetTimeControlModeLock(true);
        }

        public void UnlockTimeControl()
        {
            Campaign.Current.SetTimeControlModeLock(false);
        }
    }
}
