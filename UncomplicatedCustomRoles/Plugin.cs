using Exiled.API.Features;
using System.Collections.Generic;
using System;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Interfaces;
using Handler = UncomplicatedCustomRoles.Events.EventHandler;
using PlayerHandler = Exiled.Events.Handlers.Player;
using ServerHandler = Exiled.Events.Handlers.Server;
using Scp049Handler = Exiled.Events.Handlers.Scp049;
using System.IO;

namespace UncomplicatedCustomRoles
{
    internal class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedCustomRoles";

        public override string Prefix => "UncomplicatedCustomRoles";

        public override string Author => "FoxWorn3365, Dr.Agenda";

        public override Version Version { get; } = new(2, 0, 0, 3);

        public override Version RequiredExiledVersion { get; } = new(8, 8, 1);

        internal static Plugin Instance;

        internal Handler Handler;

        internal static Dictionary<int, ICustomRole> CustomRoles;

        internal static Dictionary<int, int> PlayerRegistry = new();

        // RolesCount: RoleId => [PlayerId, PlayerId, ...]
        internal static Dictionary<int, List<int>> RolesCount = new();

        internal static Dictionary<int, List<IUCREffect>> PermanentEffectStatus = new();

        internal static List<int> RoleSpawnQueue = new();

        internal bool DoSpawnBasicRoles = false;

        internal static HttpManager HttpManager = new("ucr");

        internal FileConfigs FileConfigs;

        public override void OnEnabled()
        {
            Instance = this;

            Handler = new();
            CustomRoles = new();

            FileConfigs = new();

            ServerHandler.RespawningTeam += Handler.OnRespawningWave;
            ServerHandler.RoundStarted += Handler.OnRoundStarted;
            PlayerHandler.Died += Handler.OnDied;
            PlayerHandler.Spawning += Handler.OnSpawning;
            PlayerHandler.Spawned += Handler.OnPlayerSpawned;
            PlayerHandler.Escaping += Handler.OnEscaping;
            PlayerHandler.UsedItem += Handler.OnItemUsed;
            PlayerHandler.Hurting += Handler.OnHurting;
            Scp049Handler.StartingRecall += Handler.OnScp049StartReviving;

            if (!File.Exists(Path.Combine(ConfigPath, "UncomplicatedCustomRoles", ".nohttp")))
            {
                HttpManager.Start();
            }

            if (Config.EnableBasicLogs)
            {
                Log.Info("===========================================");
                Log.Info(" Thanks for using UncomplicatedCustomRoles");
                Log.Info("        by FoxWorn3365 & Dr.Agenda");
                Log.Info("===========================================");
                Log.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");
            }

            if (!HttpManager.IsLatestVersion(out Version latest))
            {
                Log.Warn($"You are NOT using the latest version of UncomplicatedCustomRoles!\nCurrent: v{Version}  | Latest available: v{latest}\nDownload it from GitHub: https://github.com/FoxWorn3365/UncomplicatedCustomRoles/releases/latest");
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

            ServerHandler.RespawningTeam -= Handler.OnRespawningWave;
            ServerHandler.RoundStarted -= Handler.OnRoundStarted;
            PlayerHandler.Died -= Handler.OnDied;
            PlayerHandler.Spawning -= Handler.OnSpawning;
            PlayerHandler.Spawned -= Handler.OnPlayerSpawned;
            PlayerHandler.Escaping -= Handler.OnEscaping;
            PlayerHandler.UsedItem -= Handler.OnItemUsed;
            PlayerHandler.Hurting -= Handler.OnHurting;
            Scp049Handler.StartingRecall -= Handler.OnScp049StartReviving;

            HttpManager.Stop();

            Handler = null;
            CustomRoles = null;

            base.OnDisabled();
        }
    }
}