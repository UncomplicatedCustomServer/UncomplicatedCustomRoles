using CommandSystem;
using MEC;
using PluginAPI.Core;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Interfaces;
using UncomplicatedCustomRoles.Manager;
using Handler = UncomplicatedCustomRoles.Handlers.EventHandler;

namespace UncomplicatedCustomRoles.Commands
{
    public class Spawn : IUCRCommand
    {
        public string Name { get; } = "spawn";

        public string Description { get; } = "Spawn a player with a UCR Role";

        public PlayerPermissions RequiredPermission { get; } = PlayerPermissions.ForceclassWithoutRestrictions;

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: ucr spawn <Player Id> <Role Id>";
                return false;
            }

            if (!Round.IsRoundStarted)
            {
                response = "Sorry but you can't use this command if the round is not started!";
                return false;
            }

            Player Player = PlayerExtension.Get(arguments[0]);
            if (Player is null)
            {
                response = $"Player not found: {arguments[0]}";
                return false;
            }

            if (arguments[1] is not null)
            {
                int Id = int.Parse(arguments[1]);

                LogManager.Debug($"Selected role Id as Int32: {Id}");
                if (!CustomRole.CustomRoles.ContainsKey(Id))
                {
                    response = $"Role with the Id {Id} was not found!";
                    return false;
                } 
                else
                {
                    // Summon the player to the role
                    response = $"Player {Player.Nickname} will be spawned as {Id}!";

                    // Remove shit from the db
                    SpawnManager.ClearCustomTypes(Player);

                    if (arguments.Count > 2 && arguments[2] is not null && arguments[2] == "sync")
                    {
                        LogManager.Debug("Spawning player sync");
                        SpawnManager.SummonCustomSubclass(Player, Id, true);
                    }
                    else
                    {
                        LogManager.Debug("Spawning player async");
                        Timing.RunCoroutine(Handler.DoSpawnPlayer(Player, Id));
                    }
                    return true;
                }
            } 
            else
            {
                response = $"You must define a role Id!";
                return false;
            }
        }
    }
}