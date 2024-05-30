using Exiled.API.Features;
using System.Collections.Generic;
using System;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Interfaces;
using Handler = UncomplicatedCustomRoles.Events.EventHandler;
using PlayerHandler = Exiled.Events.Handlers.Player;
using ServerHandler = Exiled.Events.Handlers.Server;
using Scp049Handler = Exiled.Events.Handlers.Scp049;
using UncomplicatedCustomRoles.Events;
using System.IO;
using Exiled.API.Interfaces;
using Exiled.Loader;
using System.Linq;
using HarmonyLib;
using System.Reflection;
using PlayerRoles;
using Exiled.API.Features.Roles;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles
{
    internal class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedCustomRoles";

        public override string Prefix => "UncomplicatedCustomRoles";

        public override string Author => "FoxWorn3365, Dr.Agenda";

        public override Version Version { get; } = new(2, 0, 0);

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

        // Can be null if not using the RespawnTimer compatiblity or if the RespawnTimer is not loaded
        private Dictionary<string, Func<Player, string>> ReplaceHelperRespawTimer = null;

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

            Log.Debug($"Config.RespawnTimerCompatiblity: {Config.RespawnTimerCompatiblity}");
            if (Config.RespawnTimerCompatiblity)
                RespawnTimerCompatability();

            foreach (ICustomRole CustomRole in Config.CustomRoles)
            {
                SpawnManager.RegisterCustomSubclass(CustomRole);
            }

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

            if (HttpManager.IsLatestVersion())
            {
                Log.Warn($"You are NOT using the latest version of UncomplicatedCustomRoles!\nDownload it from GitHub: https://github.com/FoxWorn3365/UncomplicatedCustomRoles/releases/latest");
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

            RemoveRespawnTimerCompatiblity();

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

        private void RespawnTimerCompatability()
        {
            const string ReplaceHelperFullName = "RespawnTimer.API.Features.TimerView:ReplaceHelper";

            var propertyReplaceHelperRespawTimer = AccessTools.PropertyGetter(ReplaceHelperFullName);
            if (propertyReplaceHelperRespawTimer == null)
            {
                Log.Debug("hook to RespawnTimer, RespawnTimer.API.Features.TimerView.ReplaceHelper not found.");
                return;
            }

            ReplaceHelperRespawTimer = propertyReplaceHelperRespawTimer.Invoke(null, new object[0]) as Dictionary<string, Func<Player, string>>;
            if (ReplaceHelperRespawTimer == null)
            {
                Log.Debug("hook to RespawnTimer, faild to get the dictionary.");
                return;
            }

            ReplaceHelperRespawTimer.Add("role", GetPublicRoleName);
            Log.Debug("hook to RespawnTimer, succes.");
        }

        private void RemoveRespawnTimerCompatiblity()
        {
            // no need to check that mean RespawnTimerCompatability did not do it job
            if (ReplaceHelperRespawTimer == null)
                return;

            ReplaceHelperRespawTimer.Remove("role");
        }

        public static string GetPublicCustomRoleName(ICustomRole role, Player watcherPlayer)
        {
            if (!Plugin.Instance.Config.HiddenRolesId.TryGetValue(role.Id, out var information))
                return role.Name;

            if (information.OnlyVisibleOnOverwatch)
            {
                if (watcherPlayer.Role == RoleTypeId.Overwatch)
                {
                    return role.Name;
                }
            }
            else
            {
                if (watcherPlayer.RemoteAdminAccess)
                {
                    return role.Name;
                }
            }
            return information.RoleNameWhenHidden;
        }

        public static string GetPublicRoleName(Player player)
        {
            if (player.Role is not SpectatorRole spectator) return "...";

            var spectated = spectator.SpectatedPlayer;
            string roleName;

            if (spectated == null)
            {
                roleName = "...";
            }
            else if (spectated.TryGetCustomRole(out var customRole))
            {
                roleName = GetPublicCustomRoleName(customRole, player);
            }
            else
            {
                roleName = spectated.Role.Name;
            }
            return roleName;
        }
    }
}