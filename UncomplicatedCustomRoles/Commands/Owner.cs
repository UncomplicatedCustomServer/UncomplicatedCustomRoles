using CommandSystem;
using System.Collections.Generic;
using System.Net;
using UncomplicatedCustomRoles.Interfaces;

namespace UncomplicatedCustomRoles.Commands
{
    public class Owner : IUCRCommand
    {
        public string Name { get; } = "owner";

        public string Description { get; } = "Get the 'Server Owner' role on our Discord server";

        public PlayerPermissions RequiredPermission { get; } = PlayerPermissions.LongTermBanning;

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count != 1)
            {
                response = "Usage: ucr owner <Discord ID>";
                return false;
            }

            HttpStatusCode Response = Plugin.HttpManager.AddServerOwner(arguments[0]);

            response = Response switch
            {
                HttpStatusCode.OK => $"The request has been accepted!\nNow {arguments[0]} will be flagged as Server Owner!",
                HttpStatusCode.Forbidden => "Sorry but your server seems to not be on the public list!\nRetry in three minutes if you think that this is an error!",
                HttpStatusCode.BadRequest => "It seems that the Discord user ID is invalid!",
                HttpStatusCode.InternalServerError => "The central server is having some issues, please report this message to the Discord as a bug!",
                _ => $"The response seems to be invalid.\nRaw format: {Response}",
            };
            return true;
        }
    }
}
