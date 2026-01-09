using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Arguments.WarheadEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Patches;

namespace UncomplicatedCustomRoles.Events
{
    internal class ServerEventHandler : EventHandlerBase
    {
        internal override void OnRegistered()
        {
            ServerEvents.WaveRespawning += OnWaveRespawning;
            ServerEvents.RoundStarted += OnRoundStarted;
            ServerEvents.RoundEnded += OnRoundEnded;
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
            ServerEvents.RoundRestarted += OnRoundRestarted;

            // Warhead
            WarheadEvents.Starting += OnWarheadStarting;
        }

        internal override void OnUnregistered()
        {
            ServerEvents.WaveRespawning -= OnWaveRespawning;
            ServerEvents.RoundStarted -= OnRoundStarted;
            ServerEvents.RoundEnded -= OnRoundEnded;
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
            ServerEvents.RoundRestarted -= OnRoundRestarted;

            // Warhead
            WarheadEvents.Starting -= OnWarheadStarting;
        }

        public void OnWaitingForPlayers()
        {
            Started = false;
            Plugin.Instance.OnFinishedLoadingPlugins();
        }

        public void OnRoundStarted()
        {
            Started = true;
            FirstRoundPlayers.Clear();

            // Starts the infinite effect thing
            InfiniteEffect.Stop();
            InfiniteEffect.EffectAssociationAllowed = true;
            InfiniteEffect.Start();
        }
        
        public void OnRoundEnded(RoundEndedEventArgs _)
        {
            Started = false;
            InfiniteEffect.Terminate();
        }

        public void OnRoundRestarted()
        {
            Announcer.SavedCustomAnnouncements.Clear();
        }

        public void OnWaveRespawning(WaveRespawningEventArgs ev)
        {
            LogManager.Silent("Respawning wave");
            if (Spawn.DoHandleWave)
                foreach (Player player in ev.SpawningPlayers)
                    Spawn.SpawnQueue.Add(player.PlayerId);
            else
                Spawn.DoHandleWave = true;
        }

        public void OnWarheadStarting(WarheadStartingEventArgs ev)
        {
            if (ev.Player.ReferenceHub.GetTeam() == Team.SCPs)
                ev.IsAllowed = false;
        }
    }
}