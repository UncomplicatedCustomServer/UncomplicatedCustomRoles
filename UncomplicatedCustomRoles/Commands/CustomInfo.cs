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
using LabApi.Features.Wrappers;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Commands
{
    internal class CustomInfo : IUCRCommand
    {
        public string Name => "cinfo";

        public string Description => "Handle the Custom Role's Custom Info";

        public string RequiredPermission => "ucr.cinfo";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 3)
            {
                response = $"To execute this command provide at least 1 argument!\nUsage: ucr cinfo <Player ID> <nick|role|info> (content)";
                return false;
            }

            if (!Player.TryGet(arguments[0], out Player player))
            {
                response = "Cannot find player! Check the Player ID and try again!";
                return false;
            }

            if (!player.TryGetSummonedInstance(out SummonedCustomRole summonedInstance))
            {
                response = $"Player {player.PlayerId} is not a Custom Role!";
                return false;
            }

            string content = PlaceholderManager.ApplyPlaceholders(string.Join(" ", arguments.GetRange(2, arguments.Count - 2)), player, summonedInstance.Role);

            switch (arguments[1])
            {
                case "nick":
                    summonedInstance.CustomInfo.Nickname = content;
                    break;
                case "role":
                    summonedInstance.CustomInfo.Role = content;
                    break;
                case "info":
                    summonedInstance.CustomInfo.Info = content;
                    break;
                default:
                    response = $"Invalid field! Valid fields are: nick, role and info!";
                    return false;
            }

            response = $"Successfully updated CustomInfo of player {player.PlayerId} ({player.Nickname})!";
            return true;
        }
    }
}
