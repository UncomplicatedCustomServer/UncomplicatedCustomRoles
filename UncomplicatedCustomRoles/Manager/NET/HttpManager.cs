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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomRoles.API.Features.Messages;
using UncomplicatedCustomRoles.API.Struct;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Manager.NET;
#pragma warning disable IDE1006

internal class HttpManager
{
    /// <summary>
    ///     Create a new istance of the HttpManager
    /// </summary>
    /// <param name="prefix"></param>
    public HttpManager(string prefix)
    {
        Prefix = prefix;
        RegisterEvents();
        HttpClient = new HttpClient();
        Task.Run(LoadCreditTags);
    }

    /// <summary>
    ///     Gets the <see cref="CoroutineHandle" /> of the presence coroutine.
    /// </summary>
    public CoroutineHandle PresenceCoroutine { get; internal set; }

    /// <summary>
    ///     Gets if the feature can be activated - missing library
    /// </summary>
    public bool IsAllowed { get; internal set; } = true;

    /// <summary>
    ///     Gets the prefix of the plugin for our APIs
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    ///     Gets the <see cref="HttpClient" /> public istance
    /// </summary>
    public HttpClient HttpClient { get; }

    /// <summary>
    ///     Gets the UCS APIs endpoint
    /// </summary>
    public string Endpoint { get; } = "https://api.ucserver.it/v3/plugin";

    /// <summary>
    ///     Gets the CreditTag storage for the plugin, downloaded from our central server
    /// </summary>
    public Dictionary<string, Triplet<string, string, bool>> Credits { get; internal set; } = new();

    /// <summary>
    ///     Gets the role of the given player (as steamid@64) inside UCR
    /// </summary>
    public List<string> IsJobRole { get; } = [];

    /// <summary>
    ///     Gets every version of the plugin known by the UCS cloud
    /// </summary>
    public List<VersionInfo> Versions
    {
        get
        {
            if (_versions is null)
                LoadVersions();
            return _versions;
        }
    }

    /// <summary>
    ///     Gets the latest <see cref="Version" /> of the plugin, pre-releases included, loaded by the UCS cloud
    /// </summary>
    public Version LatestVersion
    {
        get
        {
            if (_latestVersion is null)
                LoadVersions();
            return _latestVersion;
        }
    }

    /// <summary>
    ///     Gets the latest stable (non pre-release) <see cref="Version" /> of the plugin, loaded by the UCS cloud.
    /// </summary>
    public Version LatestStableVersion
    {
        get
        {
            if (_latestStableVersion is null)
                LoadVersions();
            return _latestStableVersion;
        }
    }

    /// <summary>
    ///     Gets whether the running build is a pre-release
    /// </summary>
    public bool IsPreRelease => TryGetVersionInfo(Plugin.Instance.Version, out var info)
        ? info.PreRelease != 0
        : Plugin.Instance.Version.Revision != 0;

    private List<VersionInfo> _versions { get; set; }

    private Version _latestVersion { get; set; }

    private Version _latestStableVersion { get; set; }

    internal void RegisterEvents()
    {
        PlayerEvents.Joined += OnVerified;
    }

    internal void UnregisterEvents()
    {
        PlayerEvents.Joined -= OnVerified;
    }

    public void OnVerified(PlayerJoinedEventArgs ev)
    {
        ApplyCreditTag(ev.Player);
    }

    public string AddServerOwner(Player player, string discordId)
    {
        return HttpQuery.Post("https://api.ucserver.it/v3/owners",
            JsonSerializer.Serialize(new OwnerMessage(player, discordId)), "application/json");
    }

    public void LoadVersions()
    {
        _versions = [];
        _latestVersion = new Version();
        _latestStableVersion = new Version();

        string answer = null;

        try
        {
            answer = HttpQuery.Get($"{Endpoint}/{Prefix}/versions");
            _versions = JsonSerializer.Deserialize<List<VersionInfo>>(answer) ?? [];
        }
        catch
        {
            LogManager.Debug($"Failed to load the version list from the UCS cloud: '{answer}'");
        }

        foreach (var version in _versions)
        {
            if (!Version.TryParse(version.Name, out var parsed))
                continue;

            if (parsed > _latestVersion)
                _latestVersion = parsed;

            if (version.PreRelease == 0 && parsed > _latestStableVersion)
                _latestStableVersion = parsed;
        }

        if (_versions.Count is 0)
            LoadLatestVersionFallback();
    }

    /// <summary>
    ///     Loads the latest version from the single-value endpoint, used when the version list is unavailable.
    /// </summary>
    private void LoadLatestVersionFallback()
    {
        string answer = null;

        try
        {
            answer = HttpQuery.Get($"{Endpoint}/{Prefix}/versions/latest@text/plain");

            if (string.IsNullOrEmpty(answer) || !answer.Contains("."))
                return;

            _latestVersion = new Version(answer.Trim());

            // That endpoint doesn't tell us whether it's a pre-release, and only pre-releases ship with a non-zero
            // revision, so anything else can safely be treated as the latest stable one.
            if (_latestVersion.Revision is 0)
                _latestStableVersion = _latestVersion;
        }
        catch
        {
            LogManager.Debug($"Failed to parse the latest version received from the UCS cloud: '{answer}'");
            _latestVersion = new Version();
        }
    }

    /// <summary>
    ///     Tries to get the cloud informations about the given version of the plugin
    /// </summary>
    public bool TryGetVersionInfo(Version version, out VersionInfo info)
    {
        info = Versions.FirstOrDefault(v => Version.TryParse(v.Name, out var parsed) && parsed == version);
        return info is not null;
    }

    /// <summary>
    ///     Gets the release the current installation should be updated to, or <see langword="null" /> if there's
    ///     nothing newer to install.
    /// </summary>
    public Version GetUpdateTarget()
    {
        var current = Plugin.Instance.Version;

        if (IsPreRelease)
            current = new Version(current.Major, current.Minor, Math.Max(current.Build, 0));

        return LatestStableVersion.CompareTo(current) > 0 ? LatestStableVersion : null;
    }

    public void LoadCreditTags()
    {
        Credits = new Dictionary<string, Triplet<string, string, bool>>();
        IsJobRole.Clear();
        try
        {
            var Data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(
                HttpQuery.Get("https://api.ucserver.it/credits.json"));

            if (Data is null)
            {
                LogManager.Warn("Failed to connect to the UCS Central Server to get the credit tags informations!");
                return;
            }

            foreach (var kvp in Data.Where(kvp =>
                         kvp.Value is not null && kvp.Value.ContainsKey("role") && kvp.Value.ContainsKey("color") &&
                         kvp.Value.ContainsKey("override") && kvp.Value.ContainsKey("job")))
            {
                var role = kvp.Value["role"].GetString();
                var color = kvp.Value["color"].GetString();
                var overrideStr = kvp.Value["override"].ValueKind switch
                {
                    JsonValueKind.String => bool.Parse(kvp.Value["override"].GetString() ?? string.Empty),
                    JsonValueKind.True => true,
                    _ => false
                };
                var isJob = kvp.Value["job"].ValueKind == JsonValueKind.True;
                Credits[kvp.Key] = new Triplet<string, string, bool>(role, color, overrideStr);
                if (isJob)
                    IsJobRole.Add(kvp.Key);
            }
        }
        catch (Exception e)
        {
            LogManager.Error("An error occurred while loading the credit tags from the UCS Central Server!");
            LogManager.Debug(
                $"Failed to act HttpManager::LoadCreditTags() - {e.GetType().FullName}: {e.Message}\n{e.StackTrace}");
        }
    }

    public Triplet<string, string, bool> GetCreditTag(Player player)
    {
        if (Credits.TryGetValue(player.UserId, out var tag))
            return tag;

        return new Triplet<string, string, bool>(null, null, false);
    }

    public void ApplyCreditTag(Player player)
    {
        if (!Plugin.Instance.Config.EnableCreditTags)
            return;

        var Tag = GetCreditTag(player);

        if (!string.IsNullOrEmpty(player.ReferenceHub.serverRoles.Network_myText))
        {
            if (Credits.Any(k =>
                    k.Value.First == player.ReferenceHub.serverRoles.Network_myText &&
                    k.Value.Second == player.ReferenceHub.serverRoles.Network_myColor))
                return;

            if (!Tag.Third)
                return; // Do not override
        }

        if (Tag.First is not null && Tag.Second is not null)
        {
            player.ReferenceHub.serverRoles.SetText(Tag.First);
            player.ReferenceHub.serverRoles.SetColor(Tag.Second);
        }
    }

    public bool IsLatestVersion(out Version latest)
    {
        latest = LatestStableVersion;
        return GetUpdateTarget() is null;
    }

    public bool IsLatestVersion()
    {
        return GetUpdateTarget() is null;
    }

    internal HttpStatusCode ShareLogs(string data, out string content)
    {
        content = HttpQuery.Post($"{Endpoint}/{Prefix}/logs", JsonSerializer.Serialize(new ShareLogMessage(data)),
            "application/json");
        return content.GetStatusCode(out _);
    }
#nullable enable
    internal string VersionInfo()
    {
        return HttpQuery.Get($"{Endpoint}/{Prefix}/versions/{Plugin.Instance.Version.ToString(4)}");
    }
}