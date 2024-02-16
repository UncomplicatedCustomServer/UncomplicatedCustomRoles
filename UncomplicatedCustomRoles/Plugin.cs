using Exiled.API.Features;
using System.Collections.Generic;
using System;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Structures;
using System.Net.Http;
using Handler = UncomplicatedCustomRoles.Events.EventHandler;
using PlayerHandler = Exiled.Events.Handlers.Player;
using ServerHandler = Exiled.Events.Handlers.Server;
using MEC;
using System.IO;

namespace UncomplicatedCustomRoles
{
    internal class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedCustomRoles";
        public override string Prefix => "UncomplicatedCustomRoles";
        public override string Author => "FoxWorn3365, Dr.Agenda";
        public override Version Version { get; } = new(1, 6, 9);
        public override Version RequiredExiledVersion { get; } = new(8, 8, 0);
        public static Plugin Instance;
        internal Handler Handler;
        public static Dictionary<int, ICustomRole> CustomRoles;
        public static Dictionary<int, int> PlayerRegistry = new();
        // RolesCount: RoleId => [PlayerId, PlayerId]
        public static Dictionary<int, List<int>> RolesCount = new();
        public static Dictionary<int, List<IUCREffect>> PermanentEffectStatus = new();
        public static List<int> RoleSpawnQueue = new();
        public static HttpClient HttpClient;
        public bool DoSpawnBasicRoles = false;
        public string PresenceUrl = "https://ucs.fcosma.it/api/plugin/presence";
        public int FailedHttp;
        internal FileConfigs FileConfigs;
        public override void OnEnabled()
        {
            Instance = this;

            HttpClient = new();
            Handler = new();
            CustomRoles = new();

            FailedHttp = 0;
            FileConfigs = new();

            ServerHandler.RespawningTeam += Handler.OnRespawningWave;
            ServerHandler.RoundStarted += Handler.OnRoundStarted;
            PlayerHandler.Died += Handler.OnDied;
            PlayerHandler.Spawning += Handler.OnSpawning;
            PlayerHandler.Spawned += Handler.OnPlayerSpawned;
            PlayerHandler.Escaping += Handler.OnEscaping;
            PlayerHandler.UsedItem += Handler.OnItemUsed;

            foreach (ICustomRole CustomRole in Config.CustomRoles)
            {
                SpawnManager.RegisterCustomSubclass(CustomRole);
            }

            if (!File.Exists(Path.Combine(Paths.Configs, "UncomplicatedCustomRoles", ".nohttp")))
            {
                Log.Info($"Selecting server for UCS presence...\nFound {PresenceUrl.Replace("https://", "").Split('/')[0]}");
                Timing.RunCoroutine(Handler.DoHttpPresence());
            }

            Log.Info("===========================================");
            Log.Info(" Thanks for using UncomplicatedCustomRoles");
            Log.Info("        by FoxWorn3365 & Dr.Agenda");
            Log.Info("===========================================");
            Log.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");

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

            Handler = null;
            CustomRoles = null;

            base.OnDisabled();
        }
    }
}