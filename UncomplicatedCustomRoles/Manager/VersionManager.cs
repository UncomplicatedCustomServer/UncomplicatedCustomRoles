/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */
 
using MEC;
using System.Text.Json;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager.NET;

namespace UncomplicatedCustomRoles.Manager
{
    internal static class VersionManager
    {
        public static VersionInfo VersionInfo { get; set; }

        public static bool CorrectHash { get; private set; } = false;

#nullable enable
        public static void Init()
        {
            try
            {
                string data = Plugin.HttpManager.VersionInfo();
                HttpStatusCode code = data.GetStatusCode(out string msg);
                if (code is not HttpStatusCode.OK)
                {
                    LogManager.Warn($"Failed to gain the current version info from our central servers: API endpoint says {msg ?? "Message is null"}");
                    return;
                }

                VersionInfo = JsonSerializer.Deserialize<VersionInfo>(data);
                if (VersionInfo is null)
                {
                    LogManager.Silent($"Failed to convert API endpoint answer to VersionInfo.\nContent: {msg ?? "Message is null"}");
                    return;
                }

                if (VersionInfo.PreRelease)
                {
                    LogManager.Info($"\nNOTICE!\nYou are currently using the version v{Plugin.Instance.Version}, who's a PRE-RELEASE or an EXPERIMENTAL RELESE of UncomplicatedCustomRoles!\nLatest stable release: {Plugin.HttpManager.LatestVersion}\nNOTE: This is NOT a stable version, so there can be bugs and malfunctions, for this reason we do not recommend use in production.");
                    if (VersionInfo.ForceDebug && !(Plugin.Instance.Config?.Debug ?? true))
                    {
                        LogManager.Info("Debug logs have been activated!");
                        Plugin.Instance.Config.Debug = true;
                    }
                }
                else
                {
                    LogManager.Info($"You are using UncomplicatedCustomRoles v{VersionInfo.Name}{(VersionInfo.CustomName is not null ? $" '{VersionInfo.CustomName}'" : string.Empty)}!");
                }

                string hash = HashFile(Plugin.Instance.FilePath);
                if (hash != VersionInfo.Hash)
                    HashNotMatchMessageSender(hash);
                
                else
                    CorrectHash = true;

                if (VersionInfo.Message is not null)
                    LogManager.Info(VersionInfo.Message);

                if (VersionInfo.Recall && VersionInfo.RecallTarget is not null && VersionInfo.RecallImportant is not null && VersionInfo.RecallReason is not null)
                {
                    RecallMessageSender();
                    if ((bool)VersionInfo.RecallImportant)
                        Timing.CallContinuously(500000, RecallMessageSender);
                }
            } 
            catch (Exception e)
            {
                LogManager.Error(e.ToString());
            }
        }

        public static void HashNotMatchMessageSender(string hash) => LogManager.Error($"\nIMPORTANT ERROR!\nFAILED TO VERIFY THE PLUGIN FILE!\nThe hash of the current executable file DOES NOT MATCH the hash of that version in our database!\nOfficial hash: {VersionInfo.Hash}\nCurrent hash: {hash}", "CS0102");

        public static void RecallMessageSender() => LogManager.Warn($"\n>>> IMPORTANT NOTICE <<<\nThe current version of the plugin ({VersionInfo.Name}) HAS BEEN RECALLED FOR THE FOLLOWING REASON:\n| {VersionInfo.RecallReason?.Replace(Environment.NewLine, $"{Environment.NewLine}| ")}\nFor that reason we are asking you to PLEASE update to the next stable version, who's the {VersionInfo.RecallTarget}!\nThis version CONTAINS IMPORTANT BUGS and for that reason SWITCHING TO THE NEWER ONE IS ESSENTIAL!");

        public static string HashFile(string path)
        {
            FileStream file = new(path, FileMode.Open)
            {
                Position = 0
            };
            byte[] bytes = SHA256Managed.Create().ComputeHash(file);

            file.Close();

            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }
    }
}