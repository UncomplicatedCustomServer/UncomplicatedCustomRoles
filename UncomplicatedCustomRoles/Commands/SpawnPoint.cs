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
using System.Net;
using System.Threading.Tasks;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Manager.NET;
using SpawnPointInstance = UncomplicatedCustomRoles.API.Features.SpawnPoint;

namespace UncomplicatedCustomRoles.Commands;

internal class SpawnPoint : IUCRCommand
{
    public const string CommandHeader = "UncomplicatedCustomRoles - SpawnPoint Feature\n";

    public const string LocalError =
        "Sorry but you can't perform that action while having your spawnpoints hosted in your local folder!";

    public Dictionary<string, KeyValuePair<string, string>> SubCommands = new()
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
            "sync",
            new KeyValuePair<string, string>("",
                "Update your local SpawnPoint list by downloading it from the UCS cloud")
        },
        {
            "migrate",
            new KeyValuePair<string, string>("(NewPort) ", "Migrate current SpawnPoints to another port (but same IP)")
        },
        {
            "download",
            new KeyValuePair<string, string>("",
                "Get a link to download the current SpawnPoint list from the UCS cloud")
        },
        {
            "ip",
            new KeyValuePair<string, string>("", "Get your current IPv4/IPv6")
        }
    };

    public string Name { get; } = "spawnpoint";

    public string Description { get; } = "Manage the UCR spawnpoints";

    public string RequiredPermission { get; } = "ucr.spawnpoint";

    public bool Executor(List<string> arguments, ICommandSender sender, out string response)
    {
        var player = Player.Get(sender);

        if (player is null)
        {
            response = "You need to be a player in order to execute this command!";
            return false;
        }

        response = null;

        if (arguments.Count == 0)
        {
            response = CommandHeader;
            foreach (var command in SubCommands)
                response += $"{command.Key} {command.Value.Key}-> {command.Value.Value}\n";
        }
        else
        {
            switch (arguments[0])
            {
                case "list":
                    response =
                        $"{CommandHeader}Currently registered SpawnPoints ({SpawnPointInstance.List.Count}/{SpawnPointApiCommunicator.MaxSpawnPoints}):\n";

                    foreach (var SpawnPoint in SpawnPointInstance.List)
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

                    if (SpawnPointInstance.List.Count >= SpawnPointApiCommunicator.MaxSpawnPoints)
                    {
                        response =
                            $"You've reached the maximum number of SpawnPoints for this port!\nMaximum: {SpawnPointApiCommunicator.MaxSpawnPoints}";
                        return false;
                    }

                    new SpawnPointInstance(arguments[1], player);
                    SpawnPointApiCommunicator.AsyncPushSpawnPoints();

                    response = $"SpawnPoint {arguments[1]} successfully created!";
                    break;
                case "delete":
                    if (arguments.Count != 2)
                    {
                        response = "Wrong usage!\nucr spawnpoint delete (Name)";
                        return false;
                    }

                    if (SpawnPointInstance.TryGet(arguments[1], out var spawnPoint))
                    {
                        spawnPoint.Destroy();
                        response = "SpawnPoint successfully removed!";
                        SpawnPointApiCommunicator.AsyncPushSpawnPoints();
                    }
                    else
                    {
                        response = $"SpawnPoint '{arguments[1]}' not found!";
                    }

                    break;
                case "migrate":
                    if (SpawnPointApiCommunicator.Local)
                    {
                        response = LocalError;
                        return false;
                    }

                    if (arguments.Count < 2)
                    {
                        response = "Wrong usage!\nucr spawnpoint migrate (NewPort)";
                        return false;
                    }

                    if (!int.TryParse(arguments[1], out var newPort))
                    {
                        response = $"'{arguments[1]}' is not a valid port number!";
                        return false;
                    }

                    if (arguments.Count == 2)
                    {
                        response =
                            $"Are you sure to migrate every SpawnPoint from port {Server.Port} to port {newPort}?\nIf yes do again the command:\nucr spawnpoint migrate {arguments[1]} yes";
                        return true;
                    }

                    if (arguments.Count == 3)
                    {
                        var Status = SpawnPointApiCommunicator.PushMigrationRequest(newPort).GetStatusCode(out _);

                        if (Status is HttpStatusCode.OK)
                        {
                            response = "Migration completed!\nRefreshing the local database...";
                            SpawnPointInstance.List.Clear();
                        }
                        else
                        {
                            response = $"Migration failed!\nUCS cloud says: {Status}";
                        }
                    }

                    break;
                case "download":
                    if (SpawnPointApiCommunicator.Local)
                    {
                        response = LocalError;
                        return false;
                    }

                    var url = SpawnPointApiCommunicator.AskDownloadUrl();
                    LogManager.Info($"Download your SpawnPoint settings with this URL:\n{url}");
                    response = $"Download URL:\n{url}";
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

                    if (SpawnPointInstance.TryGet(arguments[1], out var spawn))
                    {
                        response = "Teleporting to spawnpoint...";
                        spawn.Spawn(player);
                    }
                    else
                    {
                        response = "SpawnPoint not found!";
                    }

                    break;
                case "ip":
                    if (SpawnPointApiCommunicator.Local)
                    {
                        response = LocalError;
                        return false;
                    }

                    response = $"Your IPv4/IPv6 is: {SpawnPointApiCommunicator.AskIp()}";
                    break;
                case "sync":
                    if (SpawnPointApiCommunicator.Local)
                    {
                        response = LocalError;
                        return false;
                    }

                    response = "Sync started! The SpawnPoints are being downloaded in the background...";
                    Task.Run(SpawnPointApiCommunicator.LoadFromCloud);
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