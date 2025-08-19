/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.API.Features;
using Exiled.Loader;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.Manager.NET;

namespace UncomplicatedCustomRoles.Manager
{
    internal static class VersionManager
    {
        public static VersionInfo VersionInfo { get; set; }

        public static bool CorrectHash { get; private set; } = false;

#nullable enable
        public static async void Init()
        {
            try
            {
                Tuple<HttpStatusCode, string?> data = await Plugin.HttpManager.VersionInfo();

                if (data.Item1 is not HttpStatusCode.OK || data.Item2 is null)
                {
                    LogManager.Warn($"Failed to gain the current version info from our central servers: API endpoint says {data.Item1}");
                    return;
                }

                VersionInfo = JsonConvert.DeserializeObject<VersionInfo>(data.Item2);

                if (VersionInfo is null)
                {
                    LogManager.Silent($"Failed to convert API endpoint answer to VersionInfo.\nContent: {data.Item2}");
                    return;
                }

                if (VersionInfo.PreRelease)
                {
                    LogManager.Info($"\nNOTICE!\nYou are currently using the version v{Plugin.Instance.Version}, who's a PRE-RELEASE or an EXPERIMENTAL RELESE of UncomplicatedCustomRoles!\nLatest stable release: {Plugin.HttpManager.LatestVersion}\nNOTE: This is NOT a stable version, so there can be bugs and malfunctions, for this reason we do not recommend use in production.");
                    if (VersionInfo.ForceDebug && !Log.DebugEnabled.Contains(Plugin.Instance.Assembly))
                    {
                        LogManager.Info("Debug logs have been activated!");
                        Plugin.Instance.Config.Debug = true;
                        Log.DebugEnabled.Add(Plugin.Instance.Assembly);
                    }
                }
                else
                {
                    LogManager.Info($"You are using UncomplicatedCustomRoles v{VersionInfo.Name}{(VersionInfo.CustomName is not null ? $" '{VersionInfo.CustomName}'" : string.Empty)}!");
                }

                // Check integrity
                string hash = HashFile(Plugin.Instance.Assembly.GetPath());
                if (hash != VersionInfo.Hash)
                {
                    HashNotMatchMessageSender(hash);
                    await Task.Run(async delegate
                    {
                        while (true)
                        {
                            await Task.Delay(900000);
                            HashNotMatchMessageSender(hash);
                        }
                    });
                }
                else
                    CorrectHash = true;

                if (VersionInfo.Message is not null)
                    LogManager.Info(VersionInfo.Message);

                if (VersionInfo.Recall && VersionInfo.RecallTarget is not null && VersionInfo.RecallImportant is not null && VersionInfo.RecallReason is not null)
                {
                    RecallMessageSender();
                    if ((bool)VersionInfo.RecallImportant)
                        await Task.Run(async delegate
                        {
                            while (true)
                            {
                                await Task.Delay(5000);
                                RecallMessageSender();
                            }
                        });
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
