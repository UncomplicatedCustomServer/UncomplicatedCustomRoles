using System;
using System.IO;
using UncomplicatedCustomRoles.Manager;
using Handler = UncomplicatedCustomRoles.Handlers.EventHandler;
using UncomplicatedCustomRoles.API.Features;
using HarmonyLib;
using UncomplicatedCustomRoles.Manager.NET;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using PluginAPI.Core;
using PluginAPI.Helpers;

namespace UncomplicatedCustomRoles
{
    internal class Plugin
    {
        public const string Name = "UncomplicatedCustomRoles";

        public const string Author = "FoxWorn3365";

        public const string Description = "UncomplicatedCustomRoles allows you to create custom roles for your SCP:SL server";

        public const string StringVersion = "5.0.0.1";

        public static Version Version => new(StringVersion);

        internal static Plugin Instance;

        [PluginConfig]
        public Config Config;

        internal Handler Handler;

        internal bool DoSpawnBasicRoles = false;

        internal static HttpManager HttpManager;

        internal static FileConfigs FileConfigs;

        private Harmony _harmony;

        private CustomRoleEventHandler _puppetEventHandler;

        [PluginEntryPoint(Name, StringVersion, Description, Author)]
        public void OnEnabled()
        {
            Config ??= new();

            Instance = this;

            // QoL things
            LogManager.History.Clear();
            API.Features.Escape.Bucket.Clear();

            Handler = new();
            _puppetEventHandler = new();
            FileConfigs = new();
            HttpManager = new("ucr", int.MaxValue);

            CustomRole.CustomRoles.Clear();
            
            if (!File.Exists(Path.Combine(Paths.Configs, "UncomplicatedCustomRoles", ".nohttp")))
                HttpManager.Start();

            if (Config.EnableBasicLogs)
            {
                LogManager.Info(">=========================================================<");
                LogManager.Info(" Thanks for using UncomplicatedCustomRoles - NWAPI Version");
                LogManager.Info("                 by FoxWorn3365 & Dr.Agenda");
                LogManager.Info(">=========================================================<");
                LogManager.Info("             Special thanks to:");
                LogManager.Info("  -> @timmeyxd - They gave me money in order to continue to develop this plugin while keeping it free");
                LogManager.Info("  -> @naxefir - They tested hundred of test versions in order to help me relasing the most bug-free versions");
                LogManager.Info("                   ");
                LogManager.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");
            }

            if (!HttpManager.IsLatestVersion(out Version latest))
                LogManager.Warn($"You are NOT using the latest version of UncomplicatedCustomRoles!\nCurrent: v{Version} | Latest available: v{latest}\nDownload it from GitHub: https://github.com/FoxWorn3365/UncomplicatedCustomRoles/releases/latest");

            InfiniteEffect.Stop();
            InfiniteEffect.EffectAssociationAllowed = true;
            InfiniteEffect.Start();

            // Load events
            EventManager.RegisterEvents(this, Handler);
            EventManager.RegisterEvents(this, _puppetEventHandler);
            Events.EventManager.RegisterEvents(Handler);

            FileConfigs.Welcome();
            FileConfigs.Welcome(Server.Port.ToString());
            FileConfigs.LoadAll();
            FileConfigs.LoadAll(Server.Port.ToString());

            // Start communicating with the endpoint API
            SpawnPointApiCommunicator.Init();

            // Patch with Harmony
            // no debug | Harmony.DEBUG = true;
            _harmony = new($"com.ucs.ucr_exiled-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            _harmony.PatchAll();
        }

        [PluginUnload]
        public void OnDisabled()
        {
            HttpManager.Stop();

            _harmony.UnpatchAll();
            _harmony = null;

            EventManager.UnregisterEvents(this, Handler);
            EventManager.UnregisterEvents(this, _puppetEventHandler);

            _puppetEventHandler = null;
            Handler = null;

            Instance = null;
        }

        public void OnFinishedLoadingPlugins() => ImportManager.Init();
    }
}