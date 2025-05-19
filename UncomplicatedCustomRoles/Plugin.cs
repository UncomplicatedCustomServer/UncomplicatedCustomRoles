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
using Exiled.API.Enums;
using Exiled.API.Features;
using UncomplicatedCustomRoles.Integrations;
using UncomplicatedCustomRoles.Manager;
using Handler = UncomplicatedCustomRoles.Events.EventHandler;
using PlayerHandler = Exiled.Events.Handlers.Player;
using Scp049Handler = Exiled.Events.Handlers.Scp049;
using Scp096Handler = Exiled.Events.Handlers.Scp096;
using ServerHandler = Exiled.Events.Handlers.Server;
using Scp330Handler = Exiled.Events.Handlers.Scp330;
using WarheadHandler = Exiled.Events.Handlers.Warhead;
using UncomplicatedCustomRoles.API.Features;
using HarmonyLib;
using UncomplicatedCustomRoles.Manager.NET;
using UncomplicatedCustomRoles.Patches;
using System.Threading.Tasks;

namespace UncomplicatedCustomRoles
{
    internal class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedCustomRoles";

        public override string Prefix => "UncomplicatedCustomRoles";

        public override string Author => "FoxWorn3365, Dr.Agenda";

        public override Version Version { get; } = new(7, 0, 1);

        public override Version RequiredExiledVersion { get; } = new(9, 1, 0);

        public override PluginPriority Priority => PluginPriority.Higher;

        internal static Plugin Instance;

        internal Handler Handler;

        internal static HttpManager HttpManager;

        internal Harmony _harmony;

        public override void OnEnabled()
        {
            Instance = this;

            // QoL things
            LogManager.History.Clear();
            API.Features.Escape.Bucket.Clear();

            Handler = new();
            HttpManager = new("ucr");

            CustomRole.CustomRoles.Clear();
            CustomRole.NotLoadedRoles.Clear();

            ServerHandler.RespawningTeam += Handler.OnRespawningWave;
            ServerHandler.RoundStarted += Handler.OnRoundStarted;
            ServerHandler.RoundEnded += Handler.OnRoundEnded;
            ServerHandler.WaitingForPlayers += Handler.OnWaitingForPlayers;

            PlayerHandler.ActivatingGenerator += Handler.OnGenerator;
            PlayerHandler.Dying += Handler.OnDying;
            PlayerHandler.Died += Handler.OnDied;
            PlayerHandler.SpawningRagdoll += Handler.OnRagdollSpawn;
            PlayerHandler.Spawned += Handler.OnPlayerSpawned;
            PlayerHandler.ChangingRole += Handler.OnChangingRole;
            PlayerHandler.ReceivingEffect += Handler.OnReceivingEffect;
            PlayerHandler.Escaping += Handler.OnEscaping;
            PlayerHandler.UsedItem += Handler.OnItemUsed;
            PlayerHandler.Hurting += Handler.OnHurting;
            PlayerHandler.Hurt += Handler.OnHurt;
            PlayerHandler.PickingUpItem += Handler.OnPickingUp;
            PlayerHandler.Verified += Handler.OnVerified;

            Scp049Handler.FinishingRecall += Handler.OnFinishingRecall;

            Scp096Handler.AddingTarget += Handler.OnAddingTarget;

            Scp330Handler.InteractingScp330 += Handler.OnInteractingScp330;

            WarheadHandler.Starting += Handler.OnWarheadLever;

            Task.Run(delegate
            {
                if (HttpManager.LatestVersion.CompareTo(Version) > 0)
                    LogManager.Warn($"You are NOT using the latest version of UncomplicatedCustomRoles!\nCurrent: v{Version} | Latest available: v{HttpManager.LatestVersion}\nDownload it from GitHub: https://github.com/FoxWorn3365/UncomplicatedCustomRoles/releases/latest");

                VersionManager.Init();
            });

            FileConfigs.Welcome();
            FileConfigs.Welcome(Server.Port.ToString());
            FileConfigs.LoadAll();
            FileConfigs.LoadAll(Server.Port.ToString());

            // Start communicating with the endpoint API
            SpawnPointApiCommunicator.Init();

            // Patch with Harmony
            _harmony = new($"com.ucs.ucr_exiled-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            _harmony.PatchAll();
            PlayerInfoPatch.TryPatchCedMod();

            // Register custom event handlers for custom roles
            CustomRoleEventHandler.RegisterEvents();

            RespawnTimer.Enable();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            RespawnTimer.Disable();

            CustomRoleEventHandler.UnregisterEvents();

            _harmony.UnpatchAll();

            ServerHandler.RespawningTeam -= Handler.OnRespawningWave;
            ServerHandler.RoundEnded -= Handler.OnRoundEnded;
            ServerHandler.RoundStarted -= Handler.OnRoundStarted;
            ServerHandler.WaitingForPlayers -= Handler.OnWaitingForPlayers;

            // PlayerHandler.Verified -= Handler.OnVerified;
            PlayerHandler.ActivatingGenerator -= Handler.OnGenerator;
            PlayerHandler.Dying -= Handler.OnDying;
            PlayerHandler.Died -= Handler.OnDied;
            PlayerHandler.SpawningRagdoll -= Handler.OnRagdollSpawn;
            PlayerHandler.Spawned -= Handler.OnPlayerSpawned;
            PlayerHandler.ChangingRole -= Handler.OnChangingRole;
            PlayerHandler.ReceivingEffect -= Handler.OnReceivingEffect;
            PlayerHandler.Escaping -= Handler.OnEscaping;
            PlayerHandler.UsedItem -= Handler.OnItemUsed;
            PlayerHandler.Hurting -= Handler.OnHurting;
            PlayerHandler.Hurt -= Handler.OnHurt;
            PlayerHandler.PickingUpItem -= Handler.OnPickingUp;
            PlayerHandler.Verified -= Handler.OnVerified;

            Scp049Handler.FinishingRecall -= Handler.OnFinishingRecall;

            Scp096Handler.AddingTarget -= Handler.OnAddingTarget;

            Scp330Handler.InteractingScp330 -= Handler.OnInteractingScp330;

            WarheadHandler.Starting -= Handler.OnWarheadLever;

            HttpManager.UnregisterEvents();

            Handler = null;

            Instance = null;

            base.OnDisabled();
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
                LogManager.Info("For support and to remain updated please join our Discord: https://discord.gg/5StRGu8EJV", ConsoleColor.DarkYellow);
            }
        }

        /// <summary>
        /// Invoked before EXILED starts to unload every plugin
        /// </summary>
        public void OnStartingUnloadingPlugins()
        {
            ScriptedEvents.UnregisterCustomActions();
        }
    }
}