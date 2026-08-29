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
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using MEC;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager.NET;

namespace UncomplicatedCustomRoles.Manager;

internal static class VersionManager
{
    public static VersionInfo VersionInfo { get; set; }

    public static bool CorrectHash { get; private set; }

#nullable enable
    public static Version? UpdateTarget { get; private set; }

    public static IEnumerator<float> Init()
    {
        yield return Timing.WaitUntilDone(Plugin.HttpManager.LoadVersions());
        yield return Timing.WaitUntilDone(Plugin.HttpManager.VersionInfo(LoadVersionInfo));
    }

    private static void LoadVersionInfo(HttpResponse response)
    {
        try
        {
            string data = response.Body;

            if (string.IsNullOrWhiteSpace(data))
            {
                LogManager.Silent($"The UCS cloud gave us an empty answer while asking for the version info ({response.Reason}).");
                return;
            }

            HttpStatusCode status = data.GetStatusCode(out string? msg);
            if (status is not HttpStatusCode.Unused)
            {
                LogManager.Silent($"The UCS cloud has no info about v{Plugin.Instance.Version} - HTTP {(int)status}: {msg ?? "Message is null"}");
                return;
            }

            VersionInfo = JsonSerializer.Deserialize<VersionInfo>(data);
            if (VersionInfo is null)
            {
                LogManager.Silent($"Failed to convert API endpoint answer to VersionInfo.\nContent: {msg ?? "Message is null"}");
                return;
            }

            if (VersionInfo.PreRelease != 0)
            {
                Version latestStable = Plugin.HttpManager.LatestStableVersion;
                LogManager.Info(
                    $"\nNOTICE!\nYou are currently using the version v{Plugin.Instance.Version}, who's a PRE-RELEASE or an EXPERIMENTAL RELESE of UncomplicatedCustomRoles!\nLatest stable release: {(latestStable > new Version() ? $"v{latestStable}" : "unknown")}\nNOTE: This is NOT a stable version, so there can be bugs and malfunctions, for this reason we do not recommend use in production.");
                if (VersionInfo.ForceDebug != 0 && !(Plugin.Instance.Config?.Debug ?? true))
                {
                    LogManager.Info("Debug logs have been activated!");
                    Plugin.Instance.Config.Debug = true;
                }
            }
            else
            {
                LogManager.Info(
                    $"You are using UncomplicatedCustomRoles v{VersionInfo.Name}{(VersionInfo.CustomName is not null ? $" '{VersionInfo.CustomName}'" : string.Empty)}!");
            }

            CheckForUpdates();

            string hash = HashFile(Plugin.Instance.FilePath);
            if (hash != VersionInfo.Hash)
                HashNotMatchMessageSender(hash);

            else
                CorrectHash = true;

            if (VersionInfo.Message is not null)
                LogManager.Info(VersionInfo.Message);

            if (VersionInfo.Recall != 0 && VersionInfo.RecallTarget is not null &&
                VersionInfo.RecallImportant is not null && VersionInfo.RecallReason is not null)
            {
                RecallMessageSender();
                if ((bool)VersionInfo.RecallImportant)
                    Timing.CallContinuously(500000, RecallMessageSender);
            }
        }
        catch (Exception e)
        {
            LogManager.Error("An error occurred while trying to fetch the version info from our central servers.");
            LogManager.Debug(e.ToString());
        }
    }

    public static void CheckForUpdates()
    {
        try
        {
            UpdateTarget = Plugin.HttpManager.GetUpdateTarget();

            if (UpdateTarget is null)
                return;

            LogManager.Warn(Plugin.HttpManager.IsPreReleaseVersion(UpdateTarget)
                ? $"A newer PRE-RELEASE of UncomplicatedCustomRoles is available!\nCurrent: v{Plugin.Instance.Version} | Latest pre-release: v{UpdateTarget}\n{Plugin.HttpManager.GetDownloadHint(UpdateTarget)}"
                : $"You are NOT using the latest version of UncomplicatedCustomRoles!\nCurrent: v{Plugin.Instance.Version} | Latest available: v{UpdateTarget}\n{Plugin.HttpManager.GetDownloadHint(UpdateTarget)}");
        }
        catch (Exception e)
        {
            LogManager.Error("An error occurred while checking for a newer version of the plugin.");
            LogManager.Debug(e.ToString());
        }
    }

    public static void HashNotMatchMessageSender(string hash)
    {
        LogManager.Error(
            $"\nIMPORTANT ERROR!\nFAILED TO VERIFY THE PLUGIN FILE!\nThe hash of the current executable file DOES NOT MATCH the hash of that version in our database!\nOfficial hash: {VersionInfo.Hash}\nCurrent hash: {hash}",
            "CS0102");
    }

    public static void RecallMessageSender()
    {
        string download = Version.TryParse(VersionInfo.RecallTarget, out Version? target)
            ? $"\n{Plugin.HttpManager.GetDownloadHint(target)}"
            : string.Empty;

        LogManager.Warn(
            $"\n>>> IMPORTANT NOTICE <<<\nThe current version of the plugin ({VersionInfo.Name}) HAS BEEN RECALLED FOR THE FOLLOWING REASON:\n| {VersionInfo.RecallReason?.Replace(Environment.NewLine, $"{Environment.NewLine}| ")}\nFor that reason we are asking you to PLEASE update to the next stable version, who's the {VersionInfo.RecallTarget}!{download}\nThis version CONTAINS IMPORTANT BUGS and for that reason SWITCHING TO THE NEWER ONE IS ESSENTIAL!");
    }

    public static string HashFile(string path)
    {
        using FileStream file = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using SHA256 sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(file);

        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }
}