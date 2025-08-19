/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using UncomplicatedCustomRoles.Integrations;
using UncomplicatedCustomRoles.Manager;
using Handler = UncomplicatedCustomRoles.Events.EventHandler;
using PlayerHandler = LabApi.Events.Handlers.PlayerEvents;
using Scp049Handler = LabApi.Events.Handlers.Scp049Events;
using Scp096Handler = LabApi.Events.Handlers.Scp096Events;
using Scp079Handler  = LabApi.Events.Handlers.Scp079Events;
using ServerHandler = LabApi.Events.Handlers.ServerEvents;
using WarheadHandler = LabApi.Events.Handlers.WarheadEvents;
using UncomplicatedCustomRoles.API.Features;
using HarmonyLib;
using UncomplicatedCustomRoles.Manager.NET;
using UncomplicatedCustomRoles.Patches;
using System.Threading.Tasks;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using LabApi.Features.Wrappers;
using System.Reflection;

namespace UncomplicatedCustomRoles
{
    internal class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedCustomRoles";

        public override string Description => "Customize your SCP:SL server with Custom Roles!";

        public override string Author => "FoxWorn3365, Dr.Agenda, MedveMarci";

        public override Version Version { get; } = new(9, 0, 0, 0);

        public override Version RequiredApiVersion => new(1, 1, 1);

        public override LoadPriority Priority => LoadPriority.Highest;

        public static Assembly Assembly => Assembly.GetExecutingAssembly();

        internal static Plugin Instance;

        internal static HttpManager HttpManager;

        internal Harmony _harmony;

        public override void Enable()
        {
            Instance = this;

            // QoL things
            LogManager.History.Clear();
            API.Features.Escape.Bucket.Clear();

            HttpManager = new("ucr");

            CustomRole.CustomRoles.Clear();
            CustomRole.NotLoadedRoles.Clear();

            ServerHandler.WaveRespawning += Handler.OnRespawningWave;
            ServerHandler.RoundStarted += Handler.OnRoundStarted;
            ServerHandler.RoundEnded += Handler.OnRoundEnded;
            ServerHandler.WaitingForPlayers += Handler.OnWaitingForPlayers;

            PlayerHandler.ActivatingGenerator += Handler.OnGenerator;
            PlayerHandler.Dying += Handler.OnDying;
            PlayerHandler.Death += Handler.OnDied;
            PlayerHandler.SpawningRagdoll += Handler.OnRagdollSpawn;
            PlayerHandler.ChangingRole += Handler.OnChangingRole;
            PlayerHandler.UpdatingEffect += Handler.OnReceivingEffect;
            PlayerHandler.Escaping += Handler.OnEscaping;
            PlayerHandler.UsedItem += Handler.OnItemUsed;
            PlayerHandler.Hurting += Handler.OnHurting;
            PlayerHandler.Hurt += Handler.OnHurt;
            PlayerHandler.PickingUpItem += Handler.OnPickingUp;
            PlayerHandler.RequestedRaPlayerInfo += Handler.OnRequestedRaPlayerInfo;
            PlayerHandler.RaPlayerListAddingPlayer += Handler.OnRaPlayerListAddingPlayer;
            PlayerHandler.Joined += Handler.OnJoined;
            PlayerHandler.DamagingWindow += Handler.OnDamagingWindow;

            Scp049Handler.ResurrectingBody += Handler.OnFinishingRecall;

            Scp096Handler.AddingTarget += Handler.OnAddingTarget;
            
            Scp079Handler.Recontaining += Handler.OnScp079Recontainment;

            PlayerHandler.InteractingScp330 += Handler.OnInteractingScp330;

            WarheadHandler.Starting += Handler.OnWarheadLever;

            Task.Run(delegate
            {
                if (HttpManager.LatestVersion.CompareTo(Version) > 0)
                    LogManager.Warn($"You are NOT using the latest version of UncomplicatedCustomRoles!\nCurrent: v{Version} | Latest available: v{HttpManager.LatestVersion}\nDownload it from GitHub: https://github.com/FoxWorn3365/UncomplicatedCustomRoles/releases/latest");

                VersionManager.Init();
            });

            ImportManager.Unload();

            FileConfigs.Welcome();
            FileConfigs.Welcome(Server.Port.ToString());
            FileConfigs.LoadAll();
            FileConfigs.LoadAll(Server.Port.ToString());

            // Start communicating with the endpoint API
            SpawnPointApiCommunicator.Init();

            // Patch with Harmony
            _harmony = new($"com.ucs.ucr_exiled-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            _harmony.PatchAll();

            PlayerEventPrefix.Patch(_harmony);
        }

        public override void Disable()
        {
            ScriptedEvents.UnregisterCustomActions();

            PlayerEventPrefix.Unpatch(_harmony);

            _harmony.UnpatchAll();

            ServerHandler.WaveRespawning -= Handler.OnRespawningWave;
            ServerHandler.RoundStarted -= Handler.OnRoundStarted;
            ServerHandler.RoundEnded -= Handler.OnRoundEnded;
            ServerHandler.WaitingForPlayers -= Handler.OnWaitingForPlayers;

            PlayerHandler.ActivatingGenerator -= Handler.OnGenerator;
            PlayerHandler.Dying -= Handler.OnDying;
            PlayerHandler.Death -= Handler.OnDied;
            PlayerHandler.SpawningRagdoll -= Handler.OnRagdollSpawn;
            PlayerHandler.ChangingRole -= Handler.OnChangingRole;
            PlayerHandler.UpdatingEffect -= Handler.OnReceivingEffect;
            PlayerHandler.Escaping -= Handler.OnEscaping;
            PlayerHandler.UsedItem -= Handler.OnItemUsed;
            PlayerHandler.Hurting -= Handler.OnHurting;
            PlayerHandler.Hurt -= Handler.OnHurt;
            PlayerHandler.PickingUpItem -= Handler.OnPickingUp;
            PlayerHandler.RequestedRaPlayerInfo -= Handler.OnRequestedRaPlayerInfo;
            PlayerHandler.RaPlayerListAddingPlayer -= Handler.OnRaPlayerListAddingPlayer;
            PlayerHandler.Joined -= Handler.OnJoined;
            PlayerHandler.DamagingWindow -= Handler.OnDamagingWindow;

            Scp049Handler.ResurrectingBody -= Handler.OnFinishingRecall;

            Scp096Handler.AddingTarget -= Handler.OnAddingTarget;

            Scp079Handler.Recontaining -= Handler.OnScp079Recontainment;
            
            PlayerHandler.InteractingScp330 -= Handler.OnInteractingScp330;

            WarheadHandler.Starting -= Handler.OnWarheadLever;

            HttpManager.UnregisterEvents();

            Instance = null;
        }

        /// <summary>
        /// Invoked after the server finish to load every plugin
        /// </summary>
        public void OnFinishedLoadingPlugins()
        {
            // Register ScriptedEvents integration
            ScriptedEvents.RegisterCustomActions();

            // Run the import managet
            ImportManager.Init();

            if (Config.EnableBasicLogs)
            {
                LogManager.Info($"Thanks for using UncomplicatedCustomRoles v{Version.ToString(3)} by {Author}!", ConsoleColor.Blue);
                LogManager.Info("To receive support and to stay up-to-date, join our official Discord server: https://discord.gg/5StRGu8EJV", ConsoleColor.DarkYellow);
            }
        }
    }
}