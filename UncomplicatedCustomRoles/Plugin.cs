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
using System.Reflection;
using HarmonyLib;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using MEC;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.Controllers;
using UncomplicatedCustomRoles.Events;
using UncomplicatedCustomRoles.Integrations;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Manager.NET;
using UncomplicatedCustomRoles.Patches;

namespace UncomplicatedCustomRoles;

internal class Plugin : Plugin<Config>
{
    internal static Plugin Instance;

    internal static HttpManager HttpManager;

    private Harmony _harmony;

    private bool _welcomeShown;
    public override string Name => "UncomplicatedCustomRoles";

    public override string Description => "Customize your SCP:SL server with Custom Roles!";

    public override string Author => "FoxWorn3365, Dr.Agenda, MedveMarci";

    public override Version Version { get; } = new(9, 6, 0);

    public override Version RequiredApiVersion => new(LabApiProperties.CompiledVersion);

    public override LoadPriority Priority => LoadPriority.High;

    public static Assembly Assembly => Assembly.GetExecutingAssembly();

    public override void Enable()
    {
        Instance = this;

        // QoL things
        LogManager.History.Clear();
        API.Features.Escape.Bucket.Clear();

        HttpManager = new HttpManager("ucr");

        CustomRole.CustomRoles.Clear();
        CustomRole.NotLoadedRoles.Clear();
        FlagMigrator.Migrated.Clear();

        EventHandlerBase.Register(new List<EventHandlerBase>
        {
            new ServerEventHandler(),
            new PlayerEventHandler(),
            new ScpEventHandler()
        });

        Timing.RunCoroutine(VersionManager.Init(), "UCR_Http");

        ImportManager.Unload();

        FileConfigs.Welcome();
        FileConfigs.Welcome(Server.Port.ToString());
        FileConfigs.LoadAll();
        FileConfigs.LoadAll(Server.Port.ToString());

        SpawnPointManager.Init();

        DisguiseTeam.Clear();

        TeamPatchManager.Initialize();

        _harmony = new Harmony($"com.ucs.ucr_labapi-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
        _harmony.PatchAllUncategorized();

        PlayerEventPrefix.Patch(_harmony);

        // Add presence
        if (Config.EnableTelemetry)
            Timing.RunCoroutine(Presence.PresenceCoroutine(), "UCR_Presence");
    }

    public override void Disable()
    {
        Timing.KillCoroutines("UCR_Presence");
        Timing.KillCoroutines("UCR_Http");

        ScriptedEvents.UnregisterCustomActions();

        if (_harmony is not null)
        {
            PlayerEventPrefix.Unpatch(_harmony);
            _harmony.UnpatchAll(_harmony.Id);
            _harmony = null;
        }

        PendingUnitNames.Clear();

        TeamPatchManager.Shutdown();

        EventHandlerBase.UnregisterAll();

        HttpManager?.UnregisterEvents();
        HttpManager = null;

        Instance = null;
    }

    /// <summary>
    ///     Invoked after the server finish to load every plugin
    /// </summary>
    public void OnFinishedLoadingPlugins()
    {
        // Register ScriptedEvents integration
        ScriptedEvents.RegisterCustomActions();

        // Run the import manager
        ImportManager.Init();

        if (_welcomeShown || Config is not { EnableBasicLogs: true }) return;
        _welcomeShown = true;
        LogManager.Info($"Thanks for using UncomplicatedCustomRoles v{Version} by {Author}!",
            ConsoleColor.Blue);
        LogManager.Info(
            "To receive support and to stay up-to-date, join our official Discord server: https://discord.gg/5StRGu8EJV",
            ConsoleColor.DarkYellow);
    }
}