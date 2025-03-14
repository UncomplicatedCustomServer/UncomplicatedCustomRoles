using CommandSystem;
using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Manager;
using Handler = UncomplicatedCustomRoles.Events.EventHandler;

namespace UncomplicatedCustomRoles.Commands
{
    public class Spawn : IUCRCommand
    {
        public string Name { get; } = "spawn";

        public string Description { get; } = "Spawn a player with a UCR Role";

        public string RequiredPermission { get; } = "ucr.spawn";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: ucr spawn <Player Id> <Role Id>";
                return false;
            }

            if (!Round.IsStarted)
            {
                response = "Sorry but you can't use this command if the round is not started!";
                return false;
            }

            Tuple<string, Player>[] players;

            if (arguments[0].Contains(","))
                players = arguments.Select(p => new Tuple<string, Player>(p, Player.Get(p))).ToArray();
            else
                players = new[] { new Tuple<string, Player>(arguments[0], Player.Get(arguments[0])) };

            string result = string.Empty;
            bool sync = arguments.Count > 2 && arguments[2] == "sync";

            if (arguments[1] is not null && int.TryParse(arguments[1], out int id))
                foreach (Tuple<string, Player> player in players)
                    result += $"\n{SpawnPlayer(player, id, sync)}";

            response = $"Spawning {players.Length} players as CustomRole {arguments[2]}\n{result}";
            return true;
        }

        private string SpawnPlayer(Tuple<string, Player> rawPlayer, int id, bool sync)
        {
            Player player = rawPlayer.Item2;

            if (player is null)
                return $"Player '{rawPlayer.Item1}' not found!";

            LogManager.Debug($"Selected role Id as Int32: {id}");
            if (!CustomRole.CustomRoles.ContainsKey(id))
                return $"Role with the Id {id} was not found!";
            else
            {
                // Remove shit from the db
                SpawnManager.ClearCustomTypes(player);

                if (sync)
                {
                    LogManager.Debug("Spawning player sync");
                    SpawnManager.SummonCustomSubclass(player, id, true);
                }
                else
                {
                    LogManager.Debug("Spawning player async");
                    Timing.RunCoroutine(Handler.DoSpawnPlayer(player, id));
                }

                return $"Successfully spawned player {player.Nickname} ({player.Id}) as CustomRole {id}";
            }
        }
    }
}