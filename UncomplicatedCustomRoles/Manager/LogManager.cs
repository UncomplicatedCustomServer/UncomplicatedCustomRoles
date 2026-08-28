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
using Discord;
using LabApi.Features.Console;
using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Yaml;
using MEC;
using NorthwoodLib.Pools;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Manager;

internal class LogManager
{
    // We should store the data here
    public static readonly HashSet<LogEntry> History = [];
    private static bool DebugEnabled => Plugin.Instance.Config.Debug;

    public static void Debug(string message)
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), nameof(LogLevel.Debug), message));
        if (!DebugEnabled)
            return;
        Logger.Debug(message);
    }

    public static void SmInfo(string message, string label = "Info")
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), label, message));
        Logger.Raw($"[{label}] [{Plugin.Instance.Name}] {message}", ConsoleColor.Gray);
    }

    public static void Info(string message, ConsoleColor color = ConsoleColor.Cyan)
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), nameof(LogLevel.Info), message));
        Logger.Raw($"[INFO] [{Plugin.Instance.Name}] {message}", color);
    }

    public static void Warn(string message, string error = "CS0000")
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), nameof(LogLevel.Warn), message, error));
        Logger.Warn(message);
    }

    public static void Error(string message, string error = "CS0000")
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), nameof(LogLevel.Error), message, error));
        Logger.Error(message);
    }

    public static void Silent(string message)
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "Silent", message));
    }

    public static void System(string message)
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "System", message));
    }

    internal static IEnumerator<float> SendReport(bool online, Action<HttpStatusCode, string> callback)
    {
        if (History.Count < 1)
        {
            callback?.Invoke(HttpStatusCode.Forbidden, null);
            yield break;
        }

        var builder = StringBuilderPool.Shared.Rent();

        foreach (var Element in History)
            builder.Append($"{Element}\n");

        // Now let's add the separator
        builder.Append("\n======== BEGIN CUSTOM ROLES ========\n");

        foreach (var Role in CustomRole.CustomRoles.Values)
            builder.Append($"{YamlConfigParser.Serializer.Serialize(Role)}\n\n---\n\n");

        var report = StringBuilderPool.Shared.ToStringReturn(builder);

        if (!online)
        {
            File.WriteAllText(
                Path.Combine(PathManager.Configs.FullName, $"UCR-Report-{DateTimeOffset.Now.ToUnixTimeSeconds()}.txt"),
                report);
            callback?.Invoke(HttpStatusCode.OK, null);
            yield break;
        }

        yield return Timing.WaitUntilDone(Plugin.HttpManager.ShareLogs(report,
            response => callback?.Invoke(response.Completed ? response.Body.GetStatusCode(out _) : response.Status,
                response.Body)));
    }
}