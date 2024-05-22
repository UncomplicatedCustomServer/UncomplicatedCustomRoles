using CommandSystem;
using Exiled.API.Features;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Interfaces;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Commands
{
    public class UCRRole : IUCRCommand
    {
        public string Name { get; } = "role";

        public string Description { get; } = "List all players with a custom role or see a player's custom role";

        public string RequiredPermission { get; } = "ucr.role";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count > 1)
            {
                response = "Usage: ucr role (Player ID or Name)";
                return false;
            }

            if (arguments.Count == 1)
            {
                Player Player = Player.Get(arguments[0]);
                int? RoleId = SpawnManager.TryGetCustomRole(Player);
                if (RoleId is not null)
                {
                    ICustomRole Role = Plugin.CustomRoles[RoleId.Value];
                    response = $"Player {Player.Nickname} {Player.UserId} [{Player.Id}] is the custom role {Role.Name} [{Role.Id}]";
                    return true;
                }
                response = $"Player {Player.Nickname} {Player.UserId} [{Player.Id}] is not a custom role!";
                return true;
            }
            else
            {
                response = "Custom roles of every player:\n";
                foreach (Player Player in Player.List)
                {
                    int? RoleId = SpawnManager.TryGetCustomRole(Player);
                    if (RoleId is not null)
                    {
                        ICustomRole Role = Plugin.CustomRoles[RoleId.Value];
                        response += $"\n - Player {Player.Nickname} {Player.UserId} [{Player.Id}] is the custom role {Role.Name} [{Role.Id}]";
                    } else
                    {
                        response += $"\n - Player {Player.Nickname} {Player.UserId} [{Player.Id}] is not a custom role!";
                    }
                }
                return true;
            }
        }
    }
}