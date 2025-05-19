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
using MEC;
using PlayerRoles;
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

            if (!Round.IsRoundInProgress)
            {
                response = "Sorry but you can't use this command if the round is not started!";
                return false;
            }

            IEnumerable<Tuple<string, Player>> players;

            if (arguments[0].Contains(","))
                players = arguments[0].Replace(" ", string.Empty).Split(',').Select(p => new Tuple<string, Player>(p, Player.Get(int.Parse(p))));
            else if (arguments[0] is "all")
                players = Player.List.Select(p => new Tuple<string, Player>(null, p));
            else if (arguments[0] is "spectators" or "spect")
                players = Player.List.Where(p => p.Role is RoleTypeId.Spectator or RoleTypeId.None).Select(p => new Tuple<string, Player>(null, p));
            else if (arguments[0] is "alive" or "al")
                players = Player.List.Where(p => p.Role is not RoleTypeId.Spectator or RoleTypeId.None).Select(p => new Tuple<string, Player>(null, p));
            else
                players = new[] { new Tuple<string, Player>(arguments[0], Player.Get(int.Parse(arguments[0]))) };

            string result = string.Empty;
            bool sync = arguments.Count > 2 && arguments[2] == "sync";

            if (arguments[1] is not null && int.TryParse(arguments[1], out int id))
                foreach (Tuple<string, Player> player in players)
                    result += SpawnPlayer(player, id, sync);

            response = $"Spawning {players.Count()} players as CustomRole {(sync ? "synchronously" : "asynchronously")}\n{result}";
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

                return $"Successfully spawned player {player.Nickname} ({player.PlayerId}) as CustomRole {id}";
            }
        }
    }
}