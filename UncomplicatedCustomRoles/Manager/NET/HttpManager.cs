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
using System.Text.Json;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using UncomplicatedCustomRoles.API.Features.Messages;
using UncomplicatedCustomRoles.API.Struct;

namespace UncomplicatedCustomRoles.Manager.NET;
#pragma warning disable IDE1006

internal class HttpManager
{
    private const string GitHubReleases = "https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/releases";

    private const string GitHubLatestRelease = GitHubReleases + "/latest";

    private const string DiscordInvite = "https://discord.gg/5StRGu8EJV";

    private const string CreditsEndpoint = "https://api.ucserver.it/credits.json";

    private const string OwnersEndpoint = "https://api.ucserver.it/v3/owners";

    /// <summary>
    ///     Create a new istance of the HttpManager
    /// </summary>
    /// <param name="prefix"></param>
    public HttpManager(string prefix)
    {
        Prefix = prefix;
        RegisterEvents();
        LoadCreditTags();
    }

    /// <summary>
    ///     Gets the prefix of the plugin for our APIs
    /// </summary>
    public string Prefix { get; }

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
    ///     Gets every version of the plugin known by the UCS cloud, empty until <see cref="LoadVersions" /> is done
    /// </summary>
    public List<VersionInfo> Versions { get; private set; } = [];

    /// <summary>
    ///     Gets the latest <see cref="Version" /> of the plugin, pre-releases included, loaded by the UCS cloud
    /// </summary>
    public Version LatestVersion { get; private set; } = new();

    /// <summary>
    ///     Gets the latest stable (non pre-release) <see cref="Version" /> of the plugin, loaded by the UCS cloud.
    /// </summary>
    public Version LatestStableVersion { get; private set; } = new();

    /// <summary>
    ///     Gets the latest pre-release <see cref="Version" /> of the plugin, loaded by the UCS cloud.
    /// </summary>
    public Version LatestPreRelease { get; private set; } = new();

    /// <summary>
    ///     Gets whether the running build is a pre-release
    /// </summary>
    public bool IsPreRelease => IsPreReleaseVersion(Plugin.Instance.Version);

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

    public static void AddServerOwner(Player player, string discordId, Action<HttpResponse> callback)
    {
        WebQuery.Post(OwnersEndpoint, JsonSerializer.Serialize(new OwnerMessage(player, discordId)), "application/json", callback);
    }

    internal static int CompareReleases(Version left, Version right)
    {
        int release = new Version(left.Major, left.Minor, Math.Max(left.Build, 0)).CompareTo(new Version(right.Major, right.Minor, Math.Max(right.Build, 0)));

        if (release != 0)
            return release;

        int leftPreRelease = Math.Max(left.Revision, 0);
        int rightPreRelease = Math.Max(right.Revision, 0);

        if (leftPreRelease == rightPreRelease)
            return 0;

        if (leftPreRelease is 0)
            return 1;

        return rightPreRelease is 0 ? -1 : leftPreRelease.CompareTo(rightPreRelease);
    }

    public bool IsPreReleaseVersion(Version version)
    {
        return TryGetVersionInfo(version, out VersionInfo info) ? info.PreRelease != 0 : version.Revision > 0;
    }

    public CoroutineHandle LoadVersions()
    {
        return Timing.RunCoroutine(LoadVersionsCoroutine(), "UCR_Http");
    }

    private IEnumerator<float> LoadVersionsCoroutine()
    {
        Versions = [];
        LatestVersion = new Version();
        LatestStableVersion = new Version();
        LatestPreRelease = new Version();

        yield return Timing.WaitUntilDone(WebQuery.Get($"{Endpoint}/{Prefix}/versions", LoadVersionList));

        if (Versions.Count is 0)
            yield return Timing.WaitUntilDone(WebQuery.Get($"{Endpoint}/{Prefix}/versions/latest@text/plain", LoadLatestVersionFallback));
    }

    private void LoadVersionList(HttpResponse response)
    {
        try
        {
            Versions = JsonSerializer.Deserialize<List<VersionInfo>>(response.Body) ?? [];
        }
        catch
        {
            LogManager.Debug($"Failed to load the version list from the UCS cloud ({response.Reason}): '{response.Body}'");
            Versions = [];
            return;
        }

        foreach (VersionInfo version in Versions)
        {
            if (!Version.TryParse(version.Name, out Version parsed))
                continue;

            if (CompareReleases(parsed, LatestVersion) > 0)
                LatestVersion = parsed;

            if (version.PreRelease == 0)
            {
                if (CompareReleases(parsed, LatestStableVersion) > 0)
                    LatestStableVersion = parsed;
            }
            else if (CompareReleases(parsed, LatestPreRelease) > 0)
            {
                LatestPreRelease = parsed;
            }
        }
    }

    /// <summary>
    ///     Loads the latest version from the single-value endpoint, used when the version list is unavailable.
    /// </summary>
    private void LoadLatestVersionFallback(HttpResponse response)
    {
        string answer = response.Body;

        try
        {
            if (string.IsNullOrEmpty(answer) || !answer.Contains("."))
            {
                LogManager.Debug($"The UCS cloud gave us no latest version to fall back on ({response.Reason})");
                return;
            }

            LatestVersion = new Version(answer.Trim());

            if (LatestVersion.Revision <= 0)
                LatestStableVersion = LatestVersion;
            else
                LatestPreRelease = LatestVersion;
        }
        catch
        {
            LogManager.Debug($"Failed to parse the latest version received from the UCS cloud: '{answer}'");
            LatestVersion = new Version();
        }
    }

    /// <summary>
    ///     Tries to get the cloud informations about the given version of the plugin
    /// </summary>
    public bool TryGetVersionInfo(Version version, out VersionInfo info)
    {
        info = Versions.FirstOrDefault(v => Version.TryParse(v.Name, out Version parsed) && CompareReleases(parsed, version) is 0);
        return info is not null;
    }

    private Version ResolveChannelTarget()
    {
        Version target = LatestStableVersion;

        if (IsPreRelease && CompareReleases(LatestPreRelease, target) > 0)
            target = LatestPreRelease;

        return target;
    }

    /// <summary>
    ///     Gets the release the current installation should be updated to, or <see langword="null" /> if there's
    ///     nothing newer to install.
    /// </summary>
    public Version GetUpdateTarget()
    {
        Version target = ResolveChannelTarget();
        return CompareReleases(target, Plugin.Instance.Version) > 0 ? target : null;
    }

    public string GetDownloadHint(Version version)
    {
        TryGetVersionInfo(version, out VersionInfo info);

        string link = string.IsNullOrWhiteSpace(info?.SourceLink) ? null : info.SourceLink.Trim();

        return info?.Source?.Trim().ToLowerInvariant() switch
        {
            "discord" => $"Download it from our Discord server: {link ?? DiscordInvite}",
            "other" when link is not null => $"Download it from: {link}",
            _ => $"Download it from GitHub: {link ?? (IsPreReleaseVersion(version) ? GitHubReleases : GitHubLatestRelease)}"
        };
    }

    public void LoadCreditTags()
    {
        Credits = new Dictionary<string, Triplet<string, string, bool>>();
        IsJobRole.Clear();

        WebQuery.Get(CreditsEndpoint, LoadCreditTagList);
    }

    private void LoadCreditTagList(HttpResponse response)
    {
        try
        {
            Dictionary<string, Dictionary<string, JsonElement>> Data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(response.Body);

            if (Data is null)
            {
                LogManager.Warn("Failed to connect to the UCS Central Server to get the credit tags informations!");
                return;
            }

            foreach (KeyValuePair<string, Dictionary<string, JsonElement>> kvp in Data.Where(kvp => kvp.Value is not null && kvp.Value.ContainsKey("role") && kvp.Value.ContainsKey("color") && kvp.Value.ContainsKey("override") && kvp.Value.ContainsKey("job")))
            {
                string role = kvp.Value["role"].GetString();
                string color = kvp.Value["color"].GetString();
                bool overrideStr = kvp.Value["override"].ValueKind switch
                {
                    JsonValueKind.String => bool.Parse(kvp.Value["override"].GetString() ?? string.Empty),
                    JsonValueKind.True => true,
                    _ => false
                };
                bool isJob = kvp.Value["job"].ValueKind == JsonValueKind.True;
                Credits[kvp.Key] = new Triplet<string, string, bool>(role, color, overrideStr);
                if (isJob)
                    IsJobRole.Add(kvp.Key);
            }
        }
        catch (Exception e)
        {
            LogManager.Error("An error occurred while loading the credit tags from the UCS Central Server!");
            LogManager.Debug($"Failed to act HttpManager::LoadCreditTagList() ({response.Reason}) - {e.GetType().FullName}: {e.Message}\n{e.StackTrace}");
        }
    }

    public Triplet<string, string, bool> GetCreditTag(Player player)
    {
        if (Credits.TryGetValue(player.UserId, out Triplet<string, string, bool> tag))
            return tag;

        return new Triplet<string, string, bool>(null, null, false);
    }

    public void ApplyCreditTag(Player player)
    {
        if (!Plugin.Instance.Config.EnableCreditTags)
            return;

        Triplet<string, string, bool> tag = GetCreditTag(player);

        if (!string.IsNullOrEmpty(player.ReferenceHub.serverRoles.Network_myText))
        {
            if (Credits.Any(k => k.Value.First == player.ReferenceHub.serverRoles.Network_myText && k.Value.Second == player.ReferenceHub.serverRoles.Network_myColor))
                return;

            if (!tag.Third)
                return; // Do not override
        }

        if (tag.First is not null && tag.Second is not null)
        {
            player.ReferenceHub.serverRoles.SetText(tag.First);
            player.ReferenceHub.serverRoles.SetColor(tag.Second);
        }
    }

    internal CoroutineHandle ShareLogs(string data, Action<HttpResponse> callback)
    {
        return WebQuery.Post($"{Endpoint}/{Prefix}/logs", JsonSerializer.Serialize(new ShareLogMessage(data)), "application/json", callback);
    }

    internal CoroutineHandle VersionInfo(Action<HttpResponse> callback)
    {
        return WebQuery.Get($"{Endpoint}/{Prefix}/versions/{Plugin.Instance.Version}", callback);
    }
}