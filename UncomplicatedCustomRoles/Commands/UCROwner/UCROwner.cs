using CommandSystem;
using Exiled.Permissions.Extensions;
using System;
using System.Net;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Commands.UCROwner
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class UCROwner : ParentCommand
    {
        public UCROwner() => LoadGeneratedCommands();

        public override string Command { get; } = "ucrowner";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Request the Server Owner role in the UCS Discord server - Only if the server is in the public list!";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("ucr.owner"))
            {
                response = "You do not have permission to use this command!";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "Usage: ucrowner (Discord ID)";
                return false;
            }

            HttpStatusCode Response = HttpManager.ProposeOwner(arguments.At(0));

            switch (Response)
            {
                case HttpStatusCode.OK:
                    response = $"The request has been accepted!\nNow {arguments.At(0)} will be flagged as Server Owner!";
                    break;
                case HttpStatusCode.Forbidden:
                    response = "Sorry but your server seems to not be on the public list!\nRetry in three minutes if you think that this is an error!";
                    break;
                case HttpStatusCode.BadRequest:
                    response = "It seems that the Discord user ID is invalid!";
                    break;
                case HttpStatusCode.InternalServerError:
                    response = "The central server is having some issues, please report this message to the Discord as a bug!";
                    break;
                default:
                    response = $"The response seems to be invalid.\nRaw format: {Response}";
                    break;
            }

            return true;
        }
    }
}
