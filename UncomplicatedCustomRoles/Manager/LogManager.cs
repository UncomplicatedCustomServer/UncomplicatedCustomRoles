using Discord;
using Exiled.API.Features;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using UncomplicatedCustomRoles.Interfaces;

namespace UncomplicatedCustomRoles.Manager
{
    internal class LogManager
    {
        // We should store the data here
        public static readonly List<KeyValuePair<KeyValuePair<long, LogLevel>, string>> History = new();

        public static bool MessageSent { get; internal set; } = false;

        public static void Debug(string message)
        {
            History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Debug), message));
            Log.Debug(message);
        }

        public static void Info(string message)
        {
            History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Info), message));
            Log.Info(message);
        }

        public static void Warn(string message)
        {
            History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Warn), message));
            Log.Warn(message);
        }

        public static void Error(string message)
        {
            History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Error), message));
            Log.Error(message);
        }

        public static HttpStatusCode SendReport(out HttpContent content)
        {
            content = null;

            if (MessageSent)
                return HttpStatusCode.Forbidden;

            if (History.Count < 1)
                return HttpStatusCode.Forbidden;

            string Content = string.Empty;

            foreach (KeyValuePair<KeyValuePair<long, LogLevel>, string> Element in History)
            {
                DateTimeOffset Date = DateTimeOffset.FromUnixTimeMilliseconds(Element.Key.Key);
                Content += $"[{Date.Year}-{Date.Month}-{Date.Day} {Date.Hour}:{Date.Minute}:{Date.Second} {Date.Offset}]  [{Element.Key.Value.ToString().ToUpper()}]  [UncomplicatedCustomRoles] {Element.Value}\n";
            }

            // Now let's add the separator
            Content += "======== BEGIN CUSTOM ROLES ========\n";

            foreach (ICustomRole Role in Plugin.CustomRoles.Values)
            {
                Content += $"{Loader.Serializer.Serialize(Role)}\n---\n";
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