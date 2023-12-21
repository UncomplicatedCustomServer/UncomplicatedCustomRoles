using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Structures;
using MEC;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using Exiled.Permissions.Extensions;

namespace UncomplicatedCustomRoles.Events
{
    public class EventHandler
    {
        public void OnRoundStarted()
        {
            Plugin.Instance.DoSpawnBasicRoles = false;
            Timing.CallDelayed(2, () =>
            {
                Plugin.Instance.DoSpawnBasicRoles = true;
            });
            foreach (Player Player in Player.List)
            {
                DoEvaluateSpawnForPlayer(Player);
            }
        }

        public void OnPlayerSpawned(SpawnedEventArgs Spawned)
        {
            if (!Plugin.Instance.DoSpawnBasicRoles)
            {
                return;
            }
            Log.Debug($"Player {Spawned.Player.Nickname} spawned, going to assign a role if needed!");
            Timing.CallDelayed(0.1f, () =>
            {
                DoEvaluateSpawnForPlayer(Spawned.Player);
            });
        }

        public void OnDied(DiedEventArgs Died)
        {
            SpawnManager.ClearCustomTypes(Died.Player);
        }

        public void OnSpawning(SpawningEventArgs Spawning)
        {
            SpawnManager.ClearCustomTypes(Spawning.Player);
        }

        public void OnEscaping(EscapingEventArgs Escaping)
        {
            if (Plugin.PlayerRegistry.ContainsKey(Escaping.Player.Id))
            {
                int RoleId = Plugin.PlayerRegistry[Escaping.Player.Id];
                ICustomRole Role = Plugin.CustomRoles[RoleId];
                if (!Role.CanEscape)
                {
                    Escaping.IsAllowed = false;
                }
            }
        }

        public static IEnumerator<float> DoSpawnPlayer(Player Player, int Id, bool DoBypassRoleOverwrite = true)
        {
            yield return Timing.WaitForSeconds(0.1f);
            SpawnManager.SummonCustomSubclass(Player, Id, DoBypassRoleOverwrite);
        }

        public static void DoEvaluateSpawnForPlayer(Player Player)
        {
            Dictionary<RoleTypeId, List<ICustomRole>> RolePercentage = Factory.RoleIstance();
            foreach (KeyValuePair<int, ICustomRole> Role in Plugin.CustomRoles)
            {
                if (!Role.Value.IgnoreSpawnSystem && Player.List.Count() >= Role.Value.MinPlayers)
                {
                    if (Role.Value.RequiredPermission != null && Role.Value.RequiredPermission != string.Empty && !Player.CheckPermission(Role.Value.RequiredPermission))
                    {
                        Log.Debug($"[NOTICE] Ignoring the role {Role.Value.Id} [{Role.Value.Name}] while creating the list for the player {Player.Nickname} due to: cannot [permissions].");
                        continue;
                    }
                    foreach (RoleTypeId RoleType in Role.Value.CanReplaceRoles)
                    {
                        for (int a = 0; a < Role.Value.SpawnChance; a++)
                        {
                            RolePercentage[RoleType].Add(Role.Value);
                        }
                    }
                }
            }
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
                        Timing.RunCoroutine(DoSpawnPlayer(Player, RoleId, false));
                        Plugin.RolesCount[RoleId]++;
                        Log.Debug($"Player {Player.Nickname} spawned as CustomRole {RoleId}");
                    }
                    else
                    {
                        Log.Debug($"Player {Player.Nickname} won't be spawned as CustomRole {RoleId} because it has reached the maximus number");
                    }
                }
            }
        }
    }
}