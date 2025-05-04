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
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Exiled.API.Features;

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
            response = $"Loading the JSON content to share with the developers...";

            Task.Run(() =>
            {
                HttpStatusCode Response = LogManager.SendReport(out HttpContent Content);
                try
                {
                    if (Response is HttpStatusCode.OK)
                    {
                        Dictionary<string, string> Data = JsonConvert.DeserializeObject<Dictionary<string, string>>(Plugin.HttpManager.RetriveString(Content));
                        Log.Info($"[ShareTheLog] Successfully shared the UCR logs with the developers!\nSend this Id to the developers: {Data["id"]}\n\nTook {DateTimeOffset.Now.ToUnixTimeMilliseconds() - Start}ms");
                    } 
                    else
                        Log.Info($"Failed to share the UCR logs with the developers: Server says: {Response}");
                }
                catch (Exception e) 
                { 
                    Log.Error(e.ToString()); 
                }
            });
            

            return true;
        }
    }
}