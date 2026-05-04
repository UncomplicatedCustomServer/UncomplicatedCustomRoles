/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using CommandSystem;
using System;
using System.Net;
using UncomplicatedCustomRoles.Manager;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace UncomplicatedCustomRoles.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal class LogShare : ParentCommand
    {
        public LogShare() => LoadGeneratedCommands();

        public override string Command { get; } = "ucrlogs";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Share the UCR Debug logs with the developers";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender.LogName is not "SERVER CONSOLE")
            {
                response = "Sorry but this command is reserved to the game console!";
                return false;
            }

            long Start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            response = "Loading the JSON content to share with the developers...";

            bool online = arguments.Count < 1;
            Task.Run(() =>
            {
                HttpStatusCode Response = LogManager.SendReport(out string content, online);
                try
                {
                    if (!online)
                        LogManager.Info("Logs saved to file successfully.");

                    if (Response is HttpStatusCode.OK)
                    {
                        if (string.IsNullOrEmpty(content))
                        {
                            LogManager.Error("Server returned OK but the response body was empty.");
                            return;
                        }
                        LogManager.Debug($"Received content: {content}");
                        Dictionary<string, JsonElement> Data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
                        LogManager.Info($"Successfully shared the UCR logs with the developers!\nSend this Id to the developers: {Data["id"].GetString()}\n\nTook {DateTimeOffset.Now.ToUnixTimeMilliseconds() - Start}ms");
                    }
                    else
                        LogManager.Info($"Failed to share the UCR logs with the developers: Server says: {Response}");
                }
                catch (Exception e) 
                { 
                    LogManager.Error(e.ToString()); 
                }
            });
            

            return true;
        }
    }
}