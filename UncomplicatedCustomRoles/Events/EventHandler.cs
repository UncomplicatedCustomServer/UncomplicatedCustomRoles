using Exiled.API.Features;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.Elements;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Structures;
using MEC;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Player;

namespace UncomplicatedCustomRoles.Events
{
    public class EventHandler
    {
        public void OnRoundStarted()
        {
            // Check for all subclasses and all spawn percentage
            Dictionary<RoleTypeId, List<ICustomRole>> RolePercentage = new();
            foreach (KeyValuePair<int, ICustomRole> Role in Plugin.CustomRoles)
            {
                if (!Role.Value.IgnoreSpawnSystem)
                {
                    foreach (RoleTypeId RoleType in Role.Value.CanReplaceRoles)
                    {
                        for (int a = 0; a < Role.Value.SpawnChance; a++)
                        {
                            RolePercentage[RoleType].Add(Role.Value);
                        }
                    }
                }
            }

            // Now check all the player list and assign a custom subclasses for every role
            foreach (Player Player in Player.List)
            {
                if (RolePercentage.ContainsKey(Player.Role.Type))
                {
                    // We can proceed with the chance
                    int Chance = new Random().Next(0, 100);
                    if (Chance < RolePercentage[Player.Role.Type].Count())
                    {
                        // The role exists, good, let's give the player a role
                        Timing.RunCoroutine(DoSpawnPlayer(Player, RolePercentage[Player.Role.Type].RandomItem().Id));
                        Log.Debug($"Player {Player.Nickname} spawned as CustomRole");
                    }
                }
            }
        }
        public void OnRespawningTeam(RespawningTeamEventArgs Respawn)
        {
            Dictionary<RoleTypeId, List<ICustomRole>> RolePercentage = new();
            foreach (KeyValuePair<int, ICustomRole> Role in Plugin.CustomRoles)
            {
                if (!Role.Value.IgnoreSpawnSystem)
                {
                    foreach (RoleTypeId RoleType in Role.Value.CanReplaceRoles)
                    {
                        for (int a = 0; a < Role.Value.SpawnChance; a++)
                        {
                            RolePercentage[RoleType].Add(Role.Value);
                        }
                    }
                }
            }

            // Now check all the player list and assign a custom subclasses for every role
            foreach (Player Player in Respawn.Players)
            {
                if (RolePercentage.ContainsKey(Player.Role.Type))
                {
                    // We can proceed with the chance
                    int Chance = new Random().Next(0, 100);
                    if (Chance < RolePercentage[Player.Role.Type].Count())
                    {
                        // The role exists, good, let's give the player a role
                        Timing.RunCoroutine(DoSpawnPlayer(Player, RolePercentage[Player.Role.Type].RandomItem().Id));
                        Log.Debug($"Player {Player.Nickname} spawned as CustomRole");
                    }
                }
            }
        }
        public void OnDied(DiedEventArgs Died)
        {
            if (Plugin.PlayerRegistry.ContainsKey(Died.Player.Id))
            {
                Plugin.PlayerRegistry.Remove(Died.Player.Id);
            }
        }
        public static IEnumerator<float> DoSpawnPlayer(Player Player, int Id)
        {
            yield return Timing.WaitForSeconds(0.1f);
            SpawnManager.SummonCustomSubclass(Player, Id);
        }
    }
}
