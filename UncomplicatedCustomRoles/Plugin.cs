using System;
using System.Collections.Generic;
using System.IO;
using Exiled.API.Enums;
using Exiled.API.Features;
using UncomplicatedCustomRoles.Interfaces;
using UncomplicatedCustomRoles.Manager;
using Handler = UncomplicatedCustomRoles.Events.EventHandler;
using PlayerHandler = Exiled.Events.Handlers.Player;
using Scp049Handler = Exiled.Events.Handlers.Scp049;
using ServerHandler = Exiled.Events.Handlers.Server;

namespace UncomplicatedCustomRoles
{
    internal class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedCustomRoles";

        public override string Prefix => "UncomplicatedCustomRoles";

        public override string Author => "FoxWorn3365, Dr.Agenda";

        public override Version Version { get; } = new(2, 2, 8);

        public override Version RequiredExiledVersion { get; } = new(8, 9, 4);

        public override PluginPriority Priority => PluginPriority.Higher;

        internal static Plugin Instance;

        internal Handler Handler;

        internal static Dictionary<int, ICustomRole> CustomRoles;

        // PlayerId => RoleId
        internal static Dictionary<int, int> PlayerRegistry;

        // RolesCount: RoleId => [PlayerId, PlayerId, ...]
        internal static Dictionary<int, List<int>> RolesCount;

        // PlayerId => List<IUCREffect>
        internal static Dictionary<int, List<IUCREffect>> PermanentEffectStatus;

        internal static List<int> InternalCooldownQueue;

        // List of PlayerIds
        internal static List<int> RoleSpawnQueue;

        // useful because when the spawn manager overrides the tags they will be saved here so when the role will be removed they will be reassigned
        // PlayerId => [color, name]
        internal static Dictionary<int, string[]> Tags;

        internal bool DoSpawnBasicRoles = false;

        internal static HttpManager HttpManager;

        internal static FileConfigs FileConfigs;

        internal static List<int> NicknameTracker;

        public override void OnEnabled()
        {
            Instance = this;

            // QoL things
            LogManager.History.Clear();

            Handler = new();
            FileConfigs = new();
            HttpManager = new("ucr", int.MaxValue);

            // Dictionary setup
            CustomRoles = new();
            PlayerRegistry = new();
            RoleSpawnQueue = new();
            Tags = new();
            RolesCount = new();
            PermanentEffectStatus = new();
            InternalCooldownQueue = new();
            NicknameTracker = new();

            ServerHandler.RespawningTeam += Handler.OnRespawningWave;
            ServerHandler.RoundStarted += Handler.OnRoundStarted;
            PlayerHandler.Died += Handler.OnDied;
            PlayerHandler.Spawning += Handler.OnSpawning;
            PlayerHandler.Spawned += Handler.OnPlayerSpawned;
            PlayerHandler.ChangingRole += Handler.OnChangingRole;
            //PlayerHandler.Spawned += Handler.OnSpawning;
            PlayerHandler.Escaping += Handler.OnEscaping;
            PlayerHandler.UsedItem += Handler.OnItemUsed;
            PlayerHandler.Hurting += Handler.OnHurting;
            Scp049Handler.StartingRecall += Handler.OnScp049StartReviving;
            
            RespawnTimerCompatibility.Enable();
            
            if (!File.Exists(Path.Combine(ConfigPath, "UncomplicatedCustomRoles", ".nohttp")))
            {
                HttpManager.Start();
            }

            if (Config.EnableBasicLogs)
            {
                LogManager.Info("===========================================");
                LogManager.Info(" Thanks for using UncomplicatedCustomRoles");
                LogManager.Info("        by FoxWorn3365 & Dr.Agenda");
                LogManager.Info("===========================================");
                LogManager.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");
            }

            if (!HttpManager.IsLatestVersion(out Version latest))
            {
                LogManager.Warn($"You are NOT using the latest version of UncomplicatedCustomRoles!\nCurrent: v{Version} | Latest available: v{latest}\nDownload it from GitHub: https://github.com/FoxWorn3365/UncomplicatedCustomRoles/releases/latest");
            }

            FileConfigs.Welcome();
            FileConfigs.Welcome(Server.Port.ToString());
            FileConfigs.LoadAll();
            FileConfigs.LoadAll(Server.Port.ToString());

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;

            RespawnTimerCompatibility.Disable();

            ServerHandler.RespawningTeam -= Handler.OnRespawningWave;
            ServerHandler.RoundStarted -= Handler.OnRoundStarted;
            PlayerHandler.Died -= Handler.OnDied;
            PlayerHandler.Spawning -= Handler.OnSpawning;
            PlayerHandler.Spawned -= Handler.OnPlayerSpawned;
            PlayerHandler.ChangingRole -= Handler.OnChangingRole;
            //PlayerHandler.Spawned -= Handler.OnSpawning;
            PlayerHandler.Escaping -= Handler.OnEscaping;
            PlayerHandler.UsedItem -= Handler.OnItemUsed;
            PlayerHandler.Hurting -= Handler.OnHurting;
            Scp049Handler.StartingRecall -= Handler.OnScp049StartReviving;

            HttpManager.Stop();

            Handler = null;

            base.OnDisabled();
        }
    }
}