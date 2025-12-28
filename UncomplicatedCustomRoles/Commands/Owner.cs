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
using UncomplicatedCustomRoles.Extensions;

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

            HttpStatusCode code = Plugin.HttpManager.AddServerOwner(arguments[0]).GetStatusCode(out response);
            
            response = $"{code} - {response}";
            return true;
        }
    }
}
