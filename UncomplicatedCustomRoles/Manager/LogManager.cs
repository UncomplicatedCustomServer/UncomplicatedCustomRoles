/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Discord;
using LabApi.Features.Console;
using LabApi.Loader.Features.Yaml;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Manager
{
    internal class LogManager
    {
        // We should store the data here
        public static readonly List<LogEntry> History = new();

        public static bool MessageSent { get; internal set; } = false;

        public static bool DebugEnabled => Plugin.Instance.Config.Debug;

        public static void Debug(string message)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Debug.ToString(), message));

            if (!DebugEnabled)
                return;

            Logger.Debug(message);
        }

        public static void SmInfo(string message, string label = "INFO")
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), label, message));
            Logger.Raw($"[{label}] [{Plugin.Instance.Name}] {message}", ConsoleColor.Gray);
        }

        public static void Info(string message, ConsoleColor color = ConsoleColor.Cyan)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Info.ToString(), message));
            Logger.Raw($"[INFO] [{Plugin.Instance.Name}] {message}", color);
        }

        public static void Warn(string message, string error = "CS0000")
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Warn.ToString(), message, error));
            Logger.Warn(message);
        }

        public static void Error(string message, string error = "CS0000")
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Warn.ToString(), message, error));
            Logger.Error(message);
        }

        public static void Silent(string message)
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "Silent", message));
            
            if (!DebugEnabled)
                return;

            Logger.Raw($"[SILENT DEBUG] [{Plugin.Instance.Name}] {message}", ConsoleColor.DarkYellow);
        }

        public static void System(string message) 
        {
            History.Add(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "System", message));
            
            if (!DebugEnabled)
                return;

            Logger.Raw($"[SYSTEM] [{Plugin.Instance.Name}] {message}", ConsoleColor.Blue);
        }

        internal static HttpStatusCode SendReport(out HttpContent content)
        {
            content = null;

            if (MessageSent)
                return HttpStatusCode.Forbidden;

            if (History.Count < 1)
                return HttpStatusCode.Forbidden;

            string stringContent = string.Empty;

            foreach (LogEntry Element in History)
                stringContent += $"{Element}\n";

            // Now let's add the separator
            stringContent += "\n======== BEGIN CUSTOM ROLES ========\n";

            foreach (ICustomRole Role in CustomRole.CustomRoles.Values)
                stringContent += $"{YamlConfigParser.Serializer.Serialize(Role)}\n\n---\n\n";

            HttpStatusCode Response = Plugin.HttpManager.ShareLogs(stringContent, out content);

            if (Response is HttpStatusCode.OK)
                MessageSent = true;

            return Response;
        }
    }
}