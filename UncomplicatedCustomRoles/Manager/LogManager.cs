using Discord;
using Exiled.API.Features;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Interfaces;
using UnityEngine;

namespace UncomplicatedCustomRoles.Manager
{
    internal class LogManager
    {
        // We should store the data here
        public static readonly List<KeyValuePair<KeyValuePair<long, string>, string>> History = new();

        public static bool MessageSent { get; internal set; } = false;

        public static void Debug(string message)
        {
            History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Debug.ToString()), message));
            Log.Debug(message);
        }

        public static void Info(string message)
        {
            History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Info.ToString()), message));
            Log.Info(message);
        }

        public static void Warn(string message, string error = "CS0000")
        {
            message = $"[{error}] {message}";
            History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Warn.ToString()), message));
            Log.Warn(message);
        }

        public static void Error(string message, string error = "CS0000")
        {
            message = $"[{error}] {message}";
            History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Error.ToString()), message));
            Log.Error(message);
        }

        public static void Silent(string message) => History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "SILENT"), message));

        public static void System(string message) => History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "SYSTEM"), message));

        public static HttpStatusCode SendReport(out HttpContent content)
        {
            content = null;

            if (MessageSent)
                return HttpStatusCode.Forbidden;

            if (History.Count < 1)
                return HttpStatusCode.Forbidden;

            string Content = string.Empty;

            foreach (KeyValuePair<KeyValuePair<long, string>, string> Element in History)
            {
                DateTimeOffset Date = DateTimeOffset.FromUnixTimeMilliseconds(Element.Key.Key);
                Content += $"[{Date.Year}-{Date.Month}-{Date.Day} {Date.Hour}:{Date.Minute}:{Date.Second} {Date.Offset}]  [{Element.Key.Value.ToUpper()}]  [UncomplicatedCustomRoles] {Element.Value}\n";
            }

            // Now let's add the separator
            Content += "\n======== BEGIN CUSTOM ROLES ========\n";

            foreach (ICustomRole Role in CustomRole.CustomRoles.Values)
            {
                Content += $"{Loader.Serializer.Serialize(Role)}\n\n---\n\n";
            }

            HttpStatusCode Response = Plugin.HttpManager.ShareLogs(Content, out content);

            if (Response is HttpStatusCode.OK)
            {
                MessageSent = true;
            }

            return Response;
        }
    }
}