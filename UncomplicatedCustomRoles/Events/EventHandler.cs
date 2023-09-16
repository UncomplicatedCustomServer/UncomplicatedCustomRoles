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
            Dictionary<RoleTypeId, List<ICustomRole>> RolePercentage = Factory.RoleIstance();
            foreach (KeyValuePair<int, ICustomRole> Role in Plugin.CustomRoles)
            {
                if (!Role.Value.IgnoreSpawnSystem && Role.Value.SpawnCondition == SpawnCondition.RoundStart)
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
                        int RoleId = RolePercentage[Player.Role.Type].RandomItem().Id;
                        if (Plugin.RolesCount[RoleId] < Plugin.CustomRoles[RoleId].MaxPlayers)
                        {
                            Timing.RunCoroutine(DoSpawnPlayer(Player, RoleId));
                            Plugin.RolesCount[RoleId]++;
                            Log.Debug($"Player {Player.Nickname} spawned as CustomRole {Player.Id}");
                        }
                        else
                        {
                            Log.Debug($"Player {Player.Nickname} won't be spawned as CustomRole {Player.Id} because it has reached the maximus number");
                        }
                    }
                }
            }
        }
        public void OnRespawningTeam(RespawningTeamEventArgs Respawn)
        {
            Dictionary<RoleTypeId, List<ICustomRole>> RolePercentage = Factory.RoleIstance();
            SpawnCondition SC = SpawnCondition.RoundStart;
            if (Respawn.NextKnownTeam == Respawning.SpawnableTeamType.NineTailedFox)
            {
                SC = SpawnCondition.NtfSpawn;
            } else if (Respawn.NextKnownTeam == Respawning.SpawnableTeamType.ChaosInsurgency)
            {
                SC = SpawnCondition.ChaosSpawn;
            }

            foreach (KeyValuePair<int, ICustomRole> Role in Plugin.CustomRoles)
            {
                if (!Role.Value.IgnoreSpawnSystem && Role.Value.SpawnCondition == SC)
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
                        int RoleId = RolePercentage[Player.Role.Type].RandomItem().Id;
                        if (Plugin.RolesCount[RoleId] < Plugin.CustomRoles[RoleId].MaxPlayers)
                        {
                            Timing.RunCoroutine(DoSpawnPlayer(Player, RoleId));
                            Plugin.RolesCount[RoleId]++;
                            Log.Debug($"Player {Player.Nickname} spawned as CustomRole {Player.Id}");
                        } else
                        {
                            Log.Debug($"Player {Player.Nickname} won't be spawned as CustomRole {Player.Id} because it has reached the maximus number");
                        }
                    }
                }
            }
        }
        public void OnDied(DiedEventArgs Died)
        {
            if (Plugin.PlayerRegistry.ContainsKey(Died.Player.Id))
            {
                Plugin.RolesCount[Plugin.PlayerRegistry[Died.Player.Id]]--;
                Plugin.PlayerRegistry.Remove(Died.Player.Id);
                Died.Player.CustomInfo = "";
                Died.Player.GroupName = "";
            }
        }
        public void OnSpawning(SpawningEventArgs Spawning)
        {
            if (Plugin.PlayerRegistry.ContainsKey(Spawning.Player.Id))
            {
                Plugin.RolesCount[Plugin.PlayerRegistry[Spawning.Player.Id]]--;
                Plugin.PlayerRegistry.Remove(Spawning.Player.Id);
                Spawning.Player.CustomInfo = "";
                Spawning.Player.GroupName = "";
            }
        }
        public static IEnumerator<float> DoSpawnPlayer(Player Player, int Id)
        {
            yield return Timing.WaitForSeconds(0.1f);
            SpawnManager.SummonCustomSubclass(Player, Id);
        }
    }
}
