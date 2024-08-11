using System;
using System.IO;
using Exiled.API.Enums;
using Exiled.API.Features;
using UncomplicatedCustomRoles.Integrations;
using UncomplicatedCustomRoles.Manager;
using Handler = UncomplicatedCustomRoles.Events.EventHandler;
using PlayerHandler = Exiled.Events.Handlers.Player;
using Scp049Handler = Exiled.Events.Handlers.Scp049;
using ServerHandler = Exiled.Events.Handlers.Server;
using Scp330Handler = Exiled.Events.Handlers.Scp330;
using UncomplicatedCustomRoles.API.Features;
using HarmonyLib;
using UncomplicatedCustomRoles.Manager.NET;

namespace UncomplicatedCustomRoles
{
    internal class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedCustomRoles";

        public override string Prefix => "UncomplicatedCustomRoles";

        public override string Author => "FoxWorn3365, Dr.Agenda";

        public override Version Version { get; } = new(3, 6, 0);

        public override Version RequiredExiledVersion { get; } = new(8, 11, 0);

        public override PluginPriority Priority => PluginPriority.Higher;

        internal static Plugin Instance;

        internal Handler Handler;

        internal bool DoSpawnBasicRoles = false;

        internal static HttpManager HttpManager;

        internal static FileConfigs FileConfigs;

        private Harmony _harmony;

        public override void OnEnabled()
        {
            Instance = this;

            // QoL things
            LogManager.History.Clear();
            API.Features.Escape.Bucket.Clear();

            Handler = new();
            FileConfigs = new();
            HttpManager = new("ucr", int.MaxValue);

            CustomRole.List.Clear();

            ServerHandler.RespawningTeam += Handler.OnRespawningWave;
            ServerHandler.RoundStarted += Handler.OnRoundStarted;
            ServerHandler.RoundEnded += Handler.OnRoundEnded;
            PlayerHandler.Verified += Handler.OnVerified;
            PlayerHandler.Died += Handler.OnDied;
            PlayerHandler.Spawning += Handler.OnSpawning;
            PlayerHandler.Spawned += Handler.OnPlayerSpawned;
            PlayerHandler.ChangingRole += Handler.OnChangingRole;
            PlayerHandler.ReceivingEffect += Handler.OnReceivingEffect;
            Scp330Handler.InteractingScp330 += Handler.OnInteractingScp330;
            //PlayerHandler.Spawned += Handler.OnSpawning;
            PlayerHandler.Escaping += Handler.OnEscaping;
            PlayerHandler.UsedItem += Handler.OnItemUsed;
            PlayerHandler.Hurting += Handler.OnHurting;
            Scp049Handler.FinishingRecall += Handler.OnFinishingRecall;
            
            if (!File.Exists(Path.Combine(ConfigPath, "UncomplicatedCustomRoles", ".nohttp")))
                HttpManager.Start();

            if (Config.EnableBasicLogs)
            {
                LogManager.Info("===========================================");
                LogManager.Info(" Thanks for using UncomplicatedCustomRoles");
                LogManager.Info("        by FoxWorn3365 & Dr.Agenda");
                LogManager.Info("===========================================");
                LogManager.Info("             Special thanks to:");
                LogManager.Info("  >>  @timmeyxd - They gave me money in order to continue to develop this plugin while keeping it free");
                LogManager.Info("  >>  @naxefir - They tested hundred of test versions in order to help me relasing the most bug-free versions");
                LogManager.Info("                   ");
                LogManager.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");
            }

            if (!HttpManager.IsLatestVersion(out Version latest))
                LogManager.Warn($"You are NOT using the latest version of UncomplicatedCustomRoles!\nCurrent: v{Version} | Latest available: v{latest}\nDownload it from GitHub: https://github.com/FoxWorn3365/UncomplicatedCustomRoles/releases/latest");

            InfiniteEffect.Stop();
            InfiniteEffect.EffectAssociationAllowed = true;
            InfiniteEffect.Start();

            FileConfigs.Welcome();
            FileConfigs.Welcome(Server.Port.ToString());
            FileConfigs.LoadAll();
            FileConfigs.LoadAll(Server.Port.ToString());

            // Register ScriptedEvents and RespawnTimer integration
            ScriptedEvents.RegisterCustomActions();
            RespawnTimer.Enable();

            // Start communicating with the endpoint API
            SpawnPointApiCommunicator.Init();

            // Patch with Harmony
            Harmony.DEBUG = true;
            _harmony = new("com.ucs.ucr_exiled");
            _harmony.PatchAll();

            // Run the import managet
            ImportManager.Init();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            _harmony.UnpatchAll();

            RespawnTimer.Disable();
            ScriptedEvents.UnregisterCustomActions();

            ServerHandler.RespawningTeam -= Handler.OnRespawningWave;
            ServerHandler.RoundEnded -= Handler.OnRoundEnded;
            ServerHandler.RoundStarted -= Handler.OnRoundStarted;
            PlayerHandler.Verified -= Handler.OnVerified;
            PlayerHandler.Died -= Handler.OnDied;
            PlayerHandler.Spawning -= Handler.OnSpawning;
            PlayerHandler.Spawned -= Handler.OnPlayerSpawned;
            PlayerHandler.ChangingRole -= Handler.OnChangingRole;
            PlayerHandler.ReceivingEffect -= Handler.OnReceivingEffect;
            Scp330Handler.InteractingScp330 -= Handler.OnInteractingScp330;
            //PlayerHandler.Spawned -= Handler.OnSpawning;
            PlayerHandler.Escaping -= Handler.OnEscaping;
            PlayerHandler.UsedItem -= Handler.OnItemUsed;
            PlayerHandler.Hurting -= Handler.OnHurting;
            Scp049Handler.FinishingRecall -= Handler.OnFinishingRecall;

            HttpManager.Stop();

            Handler = null;

            Instance = null;

            base.OnDisabled();
        }
    }
}