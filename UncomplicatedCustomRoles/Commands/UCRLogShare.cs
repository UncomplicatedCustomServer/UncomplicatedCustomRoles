using CommandSystem;
using System;
using System.Net;
using UncomplicatedCustomRoles.Manager;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UncomplicatedCustomRoles.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal class UCRLogShare : ParentCommand
    {
        public UCRLogShare() => LoadGeneratedCommands();

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

            HttpStatusCode Response = LogManager.SendReport(out HttpContent Content);
            Dictionary<string, string> Data = JsonConvert.DeserializeObject<Dictionary<string, string>>(Plugin.HttpManager.RetriveString(Content));

            if (Response is HttpStatusCode.OK && Data.ContainsKey("id"))
            {
                response = $"Successfully shared the UCR logs with the developers!\nSend this Id to the developers: {Data["id"]}\n\nTook {DateTimeOffset.Now.ToUnixTimeMilliseconds() - Start}ms";
            }
            else
            {
                response = $"Failed to share the UCR logs with the developers: Server says: {Response}";
            }
            

            return true;
        }
    }
}