using System;
using System.IO;
using UncomplicatedCustomRoles.Integrations;
using UncomplicatedCustomRoles.Manager;
using Handler = UncomplicatedCustomRoles.Events.EventHandler;
using UncomplicatedCustomRoles.API.Features;
using PluginAPI.Core.Attributes;
using PluginAPI.Helpers;
using PluginAPI.Enums;
using PluginAPI.Events;
using PluginAPI.Core;

namespace UncomplicatedCustomRoles
{
    internal class Plugin
    {
        [PluginConfig]
        internal Config Config;

        public readonly static Version Version = new(3, 4, 2);

        internal static Plugin Instance;

        internal Handler Handler;

        internal bool DoSpawnBasicRoles = false;

        internal static HttpManager HttpManager;

        internal static FileConfigs FileConfigs;

        [PluginEntryPoint("UncomplicatedCustomRoles", "3.4.2", "UncomplicatedCustomRoles is a plugin that allow you to create custom roles for SCP:SL", "FoxWorn3365")]
        [PluginPriority(LoadPriority.High)]
        public void OnEnabled()
        {
            Instance = this;

            // QoL things
            LogManager.History.Clear();

            Handler = new();
            FileConfigs = new();
            HttpManager = new("ucr", int.MaxValue);

            CustomRole.List.Clear();
            
            if (!File.Exists(Path.Combine(Paths.Configs, "UncomplicatedCustomRoles", ".nohttp")))
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

            // Load events
            EventManager.RegisterEvents(this, Handler);

            FileConfigs.Welcome();
            FileConfigs.Welcome(Server.Port.ToString());
            FileConfigs.LoadAll();
            FileConfigs.LoadAll(Server.Port.ToString());

            // Register ScriptedEvents and RespawnTimer integration
            ScriptedEvents.RegisterCustomActions();
            RespawnTimer.Enable();
        }

        [PluginUnload]
        public void OnDisabled()
        {
            RespawnTimer.Disable();
            ScriptedEvents.UnregisterCustomActions();

            EventManager.UnregisterAllEvents(this);

            HttpManager.Stop();

            Handler = null;

            Instance = null;
        }
    }
}