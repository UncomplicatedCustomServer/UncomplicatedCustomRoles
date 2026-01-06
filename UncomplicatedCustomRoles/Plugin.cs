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
using System.Collections.Generic;
using UncomplicatedCustomRoles.Integrations;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.API.Features;
using HarmonyLib;
using UncomplicatedCustomRoles.Manager.NET;
using UncomplicatedCustomRoles.Patches;
using System.Threading.Tasks;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using LabApi.Features.Wrappers;
using System.Reflection;
using UncomplicatedCustomRoles.Events;

namespace UncomplicatedCustomRoles
{
    internal class Plugin : Plugin<Config>
    {
        public override string Name => "UncomplicatedCustomRoles";

        public override string Description => "Customize your SCP:SL server with Custom Roles!";

        public override string Author => "FoxWorn3365, Dr.Agenda, MedveMarci";

        public override Version Version { get; } = new(9, 2, 0, 0);

        public override Version RequiredApiVersion => new(1, 1, 4);

        public override LoadPriority Priority => LoadPriority.High;

        public static Assembly Assembly => Assembly.GetExecutingAssembly();

        internal static Plugin Instance;

        internal static HttpManager HttpManager;

        private Harmony _harmony;

        public override void Enable()
        {
            Instance = this;

            // QoL things
            LogManager.History.Clear();
            API.Features.Escape.Bucket.Clear();

            HttpManager = new("ucr");

            CustomRole.CustomRoles.Clear();
            CustomRole.NotLoadedRoles.Clear();

            EventHandlerBase.Register(new List<EventHandlerBase>()
            {
                new ServerEventHandler(),
                new PlayerEventHandler(),
                new ScpEventHandler()
            });

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
            _harmony = new($"com.ucs.ucr_labapi-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            _harmony.PatchAll();

            PlayerEventPrefix.Patch(_harmony);
        }

        public override void Disable()
        {
            ScriptedEvents.UnregisterCustomActions();

            PlayerEventPrefix.Unpatch(_harmony);

            _harmony.UnpatchAll();
            
            EventHandlerBase.UnregisterAll();

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

            if (Config is not { EnableBasicLogs: true }) return;
            LogManager.Info($"Thanks for using UncomplicatedCustomRoles v{Version.ToString(3)} by {Author}!", ConsoleColor.Blue);
            LogManager.Info("To receive support and to stay up-to-date, join our official Discord server: https://discord.gg/5StRGu8EJV", ConsoleColor.DarkYellow);
        }
    }
}