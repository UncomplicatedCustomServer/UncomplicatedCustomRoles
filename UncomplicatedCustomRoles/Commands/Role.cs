using CommandSystem;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Interfaces;
using PluginAPI.Core;

namespace UncomplicatedCustomRoles.Commands
{
    public class Role : IUCRCommand
    {
        public string Name { get; } = "role";

        public string Description { get; } = "List all players with a custom role or see a player's custom role";

        public PlayerPermissions RequiredPermission { get; } = PlayerPermissions.AdminChat;

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

                if (Player is null)
                {
                    response = $"Sorry but the player {arguments[0]} does not exists!";
                    return false;
                }

                if (Player.TryGetSummonedInstance(out SummonedCustomRole summoned))
                {
                    response = $"Player {Player.Nickname} {Player.UserId} [{Player.PlayerId}] is the custom role {summoned.Role.Name} [{summoned.Role.Id}]";
                    return true;
                }
                response = $"Player {Player.Nickname} {Player.UserId} [{Player.PlayerId}] is not a custom role!";
                return true;
            }
            else
            {
                response = "Custom roles of every player:";
                foreach (Player Player in Player.GetPlayers())
                {
                    if (Player.TryGetSummonedInstance(out SummonedCustomRole summoned))
                    {
                        response += $"\n - Player {Player.Nickname} {Player.UserId} [{Player.PlayerId}] is the custom role {summoned.Role.Name} [{summoned.Role.Id}]";
                    } else
                    {
                        response += $"\n - Player {Player.Nickname} {Player.UserId} [{Player.PlayerId}] is not a custom role!";
                    }
                }
                return true;
            }
        }
    }
}