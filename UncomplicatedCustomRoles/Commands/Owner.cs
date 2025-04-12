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
using System.Collections.Generic;
using System.Net;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Commands
{
    public class Owner : IUCRCommand
    {
        public string Name { get; } = "owner";

        public string Description { get; } = "Get the 'Server Owner' role on our Discord server";

        public string RequiredPermission { get; } = "ucr.owner";

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
