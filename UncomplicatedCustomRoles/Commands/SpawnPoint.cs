using CommandSystem;
using Exiled.API.Features;
using System.Collections.Generic;
using SpawnPointInstance = UncomplicatedCustomRoles.API.Features.SpawnPoint;
using UncomplicatedCustomRoles.Manager.NET;
using System.Net;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Commands
{
    internal class SpawnPoint : IUCRCommand
    {
        public string Name { get; } = "spawnpoint";

        public string Description { get; } = "Manage the UCR spawnpoints";

        public string RequiredPermission { get; } = "ucr.spawnpoint";

        public const string CommandHeader = "UncomplicatedCustomRoles - SpawnPoint Feature\n";

        public const string LocalError = "Sorry but you can't perform that action while having your spawnpoints hosted in your local folder!";

        public Dictionary<string, KeyValuePair<string, string>> SubCommands = new()
        {
            {
                "list",
                new("", "List every registered SpawnPoint")
            },
            {
                "create",
                new("(Name) ", "Create a new SpawnPoint at your current position")
            },
            {
                "delete",
                new("(Name) ", "Delete an existing SpawnPoint")
            },
            {
                "goto",
                new("(Name) ", "Teleport yourself to a SpawnPoint")
            },
            {
                "sync",
                new("", "Update your local SpawnPoint list by downloading it from the UCS cloud")
            },
            {
                "migrate",
                new("(NewPort) ", "Migrate current SpawnPoints to another port (but same IP)")
            },
            {
                "download",
                new("", "Get a link to download the current SpawnPoint list from the UCS cloud")
            },
            {
                "ip",
                new("", "Get your current IPv4/IPv6")
            }
        }; 

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            Player Player = Player.Get(sender);

            if (Player is null)
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
                switch (arguments[0])
                {
                    case "list":
                        response = $"{CommandHeader}Currently registered SpawnPoints ({SpawnPointInstance.List.Count}/{SpawnPointApiCommunicator.MaxSpawnPoints}):\n";

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

                        if (SpawnPointInstance.List.Count >= SpawnPointApiCommunicator.MaxSpawnPoints)
                        {
                            response = $"You've reached the maximum number of SpawnPoints for this port!\nMaximum: {SpawnPointApiCommunicator.MaxSpawnPoints}";
                            return false;
                        }

                        new SpawnPointInstance(arguments[1], Player);
                        SpawnPointApiCommunicator.AsyncPushSpawnPoints();

                        response = $"SpawnPoint {arguments[1]} successfully created!";
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
                            response = "SpawnPoint successfully removed!";
                            SpawnPointApiCommunicator.AsyncPushSpawnPoints();
                        }
                        else
                            response = $"SpawnPoint '{arguments[1]}' not found!";
                        break;
                    case "migrate":
                        if (SpawnPointApiCommunicator.Local)
                            response = LocalError;


                        if (arguments.Count < 2)
                        {
                            response = "Wrong usage!\nucr spawnpoint migrate (NewPort)";
                            return false;
                        }
                        else if (arguments.Count == 2)
                        {
                            response = $"Are you sure to migrate every SpawnPoint from port {Server.Port} to port {int.Parse(arguments[1])}?\nIf yes do again the command:\nucr spawnpoint migrate {arguments[1]} yes";
                            return true;
                        }
                        else if (arguments.Count == 3)
                        {
                            HttpStatusCode Status = SpawnPointApiCommunicator.PushMigrationRequest(int.Parse(arguments[1])).StatusCode;

                            if (Status is HttpStatusCode.OK)
                            {
                                response = $"Migration completed!\nRefreshing the local database...";
                                SpawnPointInstance.List.Clear();
                            }
                            else
                                response = $"Migration failed!\nUCS cloud says: {Status}";
                        }
                        break;
                    case "download":
                        if (SpawnPointApiCommunicator.Local)
                            response = LocalError;

                        string url = SpawnPointApiCommunicator.AskDownloadUrl();
                        LogManager.Info($"Download your SpawnPoint settings with this URL:\n{SpawnPointApiCommunicator.AskDownloadUrl()}");
                        response = $"Download URL:\n{SpawnPointApiCommunicator.AskDownloadUrl()}";
                        break;
                    case "goto":
                        if (arguments.Count != 2)
                        {
                            response = "Wrong usage!\nucr spawnpoint goto (Name)";
                            return false;
                        }

                        if (!Player.IsAlive)
                        {
                            response = "You have to be alive...";
                            return false;
                        }

                        if (SpawnPointInstance.TryGet(arguments[1], out SpawnPointInstance spawn))
                        {
                            response = "Teleporting to spawnpoint...";
                            spawn.Spawn(Player);
                        }
                        else
                            response = "SpawnPoint not found!";
                        break;
                    case "ip":
                        if (SpawnPointApiCommunicator.Local)
                            response = LocalError;

                        response = $"Your IPv4/IPv6 is: {SpawnPointApiCommunicator.AskIp()}";
                        break;
                    case "sync":
                        if (SpawnPointApiCommunicator.Local)
                            response = LocalError;

                        response = "Sync done!";
                        Task.Run(SpawnPointApiCommunicator.LoadFromCloud);
                        break;
                    default:
                        response = $"SubCommand '{arguments[0]}' not found!";
                        return false;
                }

            response ??= "Internal Plugin Error - 500";
            return true;
        }
    }
}
