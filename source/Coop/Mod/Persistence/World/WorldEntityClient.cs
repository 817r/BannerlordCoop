using System;
using System.ComponentModel;
using Coop.Mod.Patch;
using JetBrains.Annotations;
using NLog;
using RailgunNet.Logic;
using TaleWorlds.CampaignSystem;

namespace Coop.Mod.Persistence.World
{
    /// <summary>
    ///     Singular instance representing global world state.
    /// </summary>
    public class WorldEntityClient : RailEntityClient<WorldState>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [NotNull] private readonly IEnvironmentClient m_Environment;

        public WorldEntityClient(IEnvironmentClient environment)
        {
            m_Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        /// <summary>
        ///     Called to request a change to the time control mode on the server.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException"></exception>
        private void RequestTimeControlChange(object instance, object value)
        {
            if (!(value is CampaignTimeControlMode mode))
            {
                throw new ArgumentException(nameof(value));
            }

            bool modelock = TimeControl.Instance.GetTimeControlModeLock();

            Logger.Trace(
                "[{tick}] Request time control mode '{mode}'.",
                Room.Tick,
                (mode, modelock));
            Room.RaiseEvent<EventTimeControl>(
                e =>
                {
                    e.EntityId = Id;
                    e.RequestedTimeControlMode = (mode, modelock);
                });
        }

        /// <summary>
        ///     Called to request a change to the time control lock on the server.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentException"></exception>
        private void RequestTimeControlLockChange(object instance, object value)
        {
            if (!(value is bool modelock))
            {
                throw new ArgumentException(nameof(value));
            }

            CampaignTimeControlMode mode = TimeControl.Instance.GetTimeControlMode();

            Logger.Trace(
                "[{tick}] Request time control mode '{mode}'.",
                Room.Tick,
                (mode, modelock));
            Room.RaiseEvent<EventTimeControl>(
                e =>
                {
                    e.EntityId = Id;
                    e.RequestedTimeControlMode = (mode, modelock);
                });
        }

        /// <summary>
        ///     Called when the world entity was added to the Railgun room.
        /// </summary>
        protected override void OnAdded()
        {
            TimeControl.Instance.TimeControlMode.SetGlobalHandler(RequestTimeControlChange);
            TimeControl.Instance.TimeControlModeLock.SetGlobalHandler(RequestTimeControlLockChange);
            State.PropertyChanged += State_PropertyChanged;
        }

        /// <summary>
        ///     Handler when any property in the world state object was changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void State_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(State.TimeControl):
                    Logger.Trace(
                        "[{tick}] Received time control mode change to '{mode}'.",
                        Room.Tick,
                        State.TimeControl);

                    TimeControl.Instance.SetTimeControlMode(State.TimeControl);
                    break;
                case nameof(State.TimeControlLock):
                    Logger.Trace(
                        "[{tick}] Received time control lock change to '{lock}'.",
                        Room.Tick,
                        State.TimeControlLock);

                    TimeControl.Instance.SetTimeControlModeLock(State.TimeControlLock);
                    break;
                case nameof(State.CampaignTimeTicks):
                    m_Environment.AuthoritativeTime =
                        Extensions.CreateCampaignTime(State.CampaignTimeTicks);
                    break;
            }
        }

        /// <summary>
        ///     Called when the world entity was removed from the Railgun room.
        /// </summary>
        protected override void OnRemoved()
        {
            m_Environment.TargetPosition.RemoveGlobalHandler();
            State.PropertyChanged -= State_PropertyChanged;
        }

        public override string ToString()
        {
            return $"World ({Id}): {State}.";
        }
    }
}
