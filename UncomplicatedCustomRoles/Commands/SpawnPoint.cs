/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Manager;
using SpawnPointInstance = UncomplicatedCustomRoles.API.Features.SpawnPoint;

namespace UncomplicatedCustomRoles.Commands;

internal class SpawnPoint : IUCRCommand
{
    public const string CommandHeader = "UncomplicatedCustomRoles - SpawnPoint Feature\n";

    public readonly Dictionary<string, KeyValuePair<string, string>> SubCommands = new()
    {
        {
            "list",
            new KeyValuePair<string, string>("", "List every registered SpawnPoint")
        },
        {
            "create",
            new KeyValuePair<string, string>("(Name) ", "Create a new SpawnPoint at your current position")
        },
        {
            "delete",
            new KeyValuePair<string, string>("(Name) ", "Delete an existing SpawnPoint")
        },
        {
            "goto",
            new KeyValuePair<string, string>("(Name) ", "Teleport yourself to a SpawnPoint")
        },
        {
            "reload",
            new KeyValuePair<string, string>("", "Reload the SpawnPoint list from the local file, discarding every unsaved change")
        },
        {
            "path",
            new KeyValuePair<string, string>("", "Show where the SpawnPoints of this server are stored")
        }
    };

    public string Name { get; } = "spawnpoint";

    public string Description { get; } = "Manage the UCR spawnpoints";

    public string RequiredPermission { get; } = "ucr.spawnpoint";

    public bool Executor(List<string> arguments, ICommandSender sender, out string response)
    {
        Player player = Player.Get(sender);

        if (player is null)
        {
            response = "You need to be a player in order to execute this command!";
            return false;
        }

        response = null;

        if (arguments.Count == 0)
        {
            response = CommandHeader;
            foreach (KeyValuePair<string, KeyValuePair<string, string>> command in SubCommands)
                response += $"{command.Key} {command.Value.Key}-> {command.Value.Value}\n";
        }
        else
        {
            switch (arguments[0])
            {
                case "list":
                    response = $"{CommandHeader}Currently registered SpawnPoints ({SpawnPointInstance.List.Count}):\n";

                    foreach (SpawnPointInstance SpawnPoint in SpawnPointInstance.List)
                        response += $"- {SpawnPoint}\n";

                    break;
                case "create":
                    if (arguments.Count != 2)
                    {
                        response = "Wrong usage!\nucr spawnpoint create (Name)";
                        return false;
                    }

                    if (SpawnPointInstance.TryGet(arguments[1], out _))
                    {
                        response = $"A SpawnPoint with the name '{arguments[1]}' is already registered!";
                        return false;
                    }

                    new SpawnPointInstance(arguments[1], player);

                    response = SpawnPointManager.Save() ? $"SpawnPoint {arguments[1]} successfully created!" : $"SpawnPoint {arguments[1]} created!\nThe SpawnPoint list has been updated but it could NOT be saved on the disk: check the server console!";
                    break;
                case "delete":
                    if (arguments.Count != 2)
                    {
                        response = "Wrong usage!\nucr spawnpoint delete (Name)";
                        return false;
                    }

                    if (SpawnPointInstance.TryGet(arguments[1], out SpawnPointInstance spawnPoint))
                    {
                        spawnPoint.Destroy();
                        response = SpawnPointManager.Save() ? "SpawnPoint successfully removed!" : "SpawnPoint removed!\nThe SpawnPoint list has been updated but it could NOT be saved on the disk: check the server console!";
                    }
                    else
                    {
                        response = $"SpawnPoint '{arguments[1]}' not found!";
                    }

                    break;
                case "goto":
                    if (arguments.Count != 2)
                    {
                        response = "Wrong usage!\nucr spawnpoint goto (Name)";
                        return false;
                    }

                    if (!player.IsAlive)
                    {
                        response = "You have to be alive...";
                        return false;
                    }

                    if (SpawnPointInstance.TryGet(arguments[1], out SpawnPointInstance spawn))
                    {
                        response = "Teleporting to spawnpoint...";
                        spawn.Spawn(player);
                    }
                    else
                    {
                        response = "SpawnPoint not found!";
                    }

                    break;
                case "reload":
                case "sync":
                    response = $"Reloaded {SpawnPointManager.Load()} SpawnPoints from the local storage!";
                    break;
                case "path":
                    response = $"Your SpawnPoints are stored in:\n{SpawnPointManager.FilePath}";
                    break;
                default:
                    response = $"SubCommand '{arguments[0]}' not found!";
                    return false;
            }
        }

        response ??= "Internal Plugin Error - 500";
        return true;
    }
}