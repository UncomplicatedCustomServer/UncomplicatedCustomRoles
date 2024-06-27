using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Interfaces;
using MEC;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using Exiled.Permissions.Extensions;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Scp049;
using Exiled.API.Enums;
using UnityEngine;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Events
{
    public class EventHandler
    {
        internal CoroutineHandle EffectCoroutine;

        public void OnRoundStarted()
        {
            Plugin.PlayerRegistry = new();
            Plugin.RolesCount = new();
            foreach (KeyValuePair<int, ICustomRole> Data in Plugin.CustomRoles)
            {
                Plugin.RolesCount[Data.Key] = new();
            }
            Plugin.Instance.DoSpawnBasicRoles = false;
            if (EffectCoroutine.IsRunning)
            {
                Timing.KillCoroutines(EffectCoroutine);
            }
            EffectCoroutine = Timing.RunCoroutine(DoSetInfiniteEffectToPlayers());
            Timing.CallDelayed(5, () =>
            {
                Plugin.Instance.DoSpawnBasicRoles = true;
            });
            foreach (Player Player in Player.List.Where(player => !player.IsNPC))
            {
                ICustomRole Role = DoEvaluateSpawnForPlayer(Player);

                if (Role is not null)
                    Timing.RunCoroutine(DoSpawnPlayer(Player, Role.Id));
            }
        }

        public void OnPlayerSpawned(SpawnedEventArgs Spawned)
        {
            if (Spawned.Player.HasCustomRole() && Plugin.InternalCooldownQueue.Contains(Spawned.Player.Id))
            {
                Plugin.InternalCooldownQueue.Remove(Spawned.Player.Id);
            }
        }

        public void OnScp049StartReviving(StartingRecallEventArgs Recall)
        {
            if (Plugin.CustomRoles.Where(cr => cr.Value.SpawnSettings is not null && cr.Value.SpawnSettings.CanReplaceRoles.Contains(RoleTypeId.Scp0492)).Count() > 0) {
                Plugin.RoleSpawnQueue.Add(Recall.Target.Id);
            }
        }

        public void OnDied(DiedEventArgs Died)
        {
            SpawnManager.ClearCustomTypes(Died.Player);
        }

        public void OnSpawning(SpawningEventArgs Spawning)
        {
            Log.Warn("Called SPAWNING event");

            if (Plugin.InternalCooldownQueue.Contains(Spawning.Player.Id))
                return;

            if (Plugin.PlayerRegistry.ContainsKey(Spawning.Player.Id))
                return;

            if (!Plugin.Instance.DoSpawnBasicRoles)
                return;

            if (Spawning.Player.IsNPC)
                return;

            string LogReason = string.Empty;
            if (Plugin.Instance.Config.AllowOnlyNaturalSpawns && !Plugin.RoleSpawnQueue.Contains(Spawning.Player.Id))
            {
                LogManager.Debug("The player is not in the queue for respawning!");
                return;
            }
            else if (Plugin.RoleSpawnQueue.Contains(Spawning.Player.Id))
            {
                Plugin.RoleSpawnQueue.Remove(Spawning.Player.Id);
                LogReason = " [going with a respawn wave OR 049 revival]";
            }

            LogManager.Debug($"Player {Spawning.Player.Nickname} spawned{LogReason}, going to assign a role if needed!");

            ICustomRole Role = DoEvaluateSpawnForPlayer(Spawning.Player);

            if (Role is not null)
                SpawnManager.SummonCustomSubclass(Spawning.Player, Role.Id);

            Log.Debug($"Evaluated custom role for player {Spawning.Player.Nickname} - found: {Role.Id} ({Role.Name})");
        }

        public void OnHurting(HurtingEventArgs Hurting)
        {
            if (Plugin.PlayerRegistry.ContainsKey(Hurting.Player.Id))
            {
                ICustomRole Role = Plugin.CustomRoles[Plugin.PlayerRegistry[Hurting.Player.Id]];
                Hurting.DamageHandler.Damage *= Role.DamageMultiplier;
            }
        }

        public void OnEscaping(EscapingEventArgs Escaping)
        {
            LogManager.Debug($"Player {Escaping.Player.Nickname} triggered the escaping event as {Escaping.Player.Role.Name}");

            if (Plugin.PlayerRegistry.ContainsKey(Escaping.Player.Id))
            {
                LogManager.Debug($"Player IS a custom role: {Plugin.PlayerRegistry[Escaping.Player.Id]}");
                ICustomRole Role = Plugin.CustomRoles[Plugin.PlayerRegistry[Escaping.Player.Id]];

                if (!Role.CanEscape)
                {
                    LogManager.Debug($"Player with the role {Role.Id} ({Role.Name}) can't escape, so nuh uh!");
                    Escaping.IsAllowed = false;
                    return;
                }

                if (Role.CanEscape && (Role.RoleAfterEscape is null || Role.RoleAfterEscape.Length < 2))
                {
                    LogManager.Debug($"Player with the role {Role.Id} ({Role.Name}) evaluated for a natural respawn!");
                    Escaping.IsAllowed = true;
                    return;
                }

                // Try to set the role
                RoleTypeId? NewRole = SpawnManager.ParseEscapeRole(Role.RoleAfterEscape, Escaping.Player);

                if (NewRole is not null)
                {
                    Escaping.IsAllowed = true;
                    Escaping.NewRole = (RoleTypeId)NewRole;
                }
                else
                {
                    Escaping.IsAllowed = false;
                }
            }
        }

        public void OnRespawningWave(RespawningTeamEventArgs Respawn)
        {
            foreach (Player Player in Respawn.Players)
            {
                Plugin.RoleSpawnQueue.Add(Player.Id);
            }
        }

        public void OnItemUsed(UsedItemEventArgs UsedItem)
        {
            if (UsedItem.Player is not null && UsedItem.Player.HasCustomRole() && Plugin.PermanentEffectStatus.ContainsKey(UsedItem.Player.Id) && UsedItem.Item.Type == ItemType.SCP500)
            {
                foreach (IUCREffect Effect in Plugin.PermanentEffectStatus[UsedItem.Player.Id])
                {
                    if (Effect.Removable)
                    {
                        Plugin.PermanentEffectStatus[UsedItem.Player.Id].Remove(Effect);
                    }
                }
                SpawnManager.SetAllActiveEffect(UsedItem.Player);
            }
        }

        public IEnumerator<float> DoSetInfiniteEffectToPlayers()
        {
            while (Round.InProgress)
            {
                foreach (Player Player in Player.List.Where(player => Plugin.PermanentEffectStatus.ContainsKey(player.Id) && player.IsAlive && Plugin.PlayerRegistry.ContainsKey(player.Id)))
                {
                    SpawnManager.SetAllActiveEffect(Player);
                }

                // Here we can see and trigger role for SCPs escape event
                foreach (Player Player in Player.List.Where(player => player.IsScp && Vector3.Distance(new(123.85f, 988.8f, 18.9f), player.Position) < 2.5f)) 
                {
                    LogManager.Debug("Calling respawn event for plauer -> position");
                    // Let's make this SCP escape
                    OnEscaping(new(Player, RoleTypeId.ChaosConscript, EscapeScenario.None));
                }

                yield return Timing.WaitForSeconds(2f);
            }
        }

        public static IEnumerator<float> DoSpawnPlayer(Player Player, int Id, bool DoBypassRoleOverwrite = true)
        {
            yield return Timing.WaitForSeconds(0.1f);
            SpawnManager.SummonCustomSubclass(Player, Id, DoBypassRoleOverwrite);
        }

        public static ICustomRole DoEvaluateSpawnForPlayer(Player Player)
        {
            Dictionary<RoleTypeId, List<ICustomRole>> RolePercentage = new()
            {
                { RoleTypeId.ClassD, new() },
                { RoleTypeId.Scientist, new() },
                { RoleTypeId.NtfPrivate, new() },
                { RoleTypeId.NtfSergeant, new() },
                { RoleTypeId.NtfCaptain, new() },
                { RoleTypeId.NtfSpecialist, new() },
                { RoleTypeId.ChaosConscript, new() },
                { RoleTypeId.ChaosMarauder, new() },
                { RoleTypeId.ChaosRepressor, new() },
                { RoleTypeId.ChaosRifleman, new() },
                { RoleTypeId.Tutorial, new() },
                { RoleTypeId.Scp049, new() },
                { RoleTypeId.Scp0492, new() },
                { RoleTypeId.Scp079, new() },
                { RoleTypeId.Scp173, new() },
                { RoleTypeId.Scp939, new() },
                { RoleTypeId.Scp096, new() },
                { RoleTypeId.Scp106, new() },
                { RoleTypeId.Scp3114, new() },
                { RoleTypeId.FacilityGuard, new() }
            };

            foreach (ICustomRole Role in Plugin.CustomRoles.Values.Where(cr => cr.SpawnSettings is not null))
            {
                if (!Role.IgnoreSpawnSystem && Player.List.Count() >= Role.SpawnSettings.MinPlayers)
                {
                    if (Role.SpawnSettings.RequiredPermission != null && Role.SpawnSettings.RequiredPermission != string.Empty && !Player.CheckPermission(Role.SpawnSettings.RequiredPermission))
                    {
                        LogManager.Debug($"[NOTICE] Ignoring the role {Role.Id} [{Role.Name}] while creating the list for the player {Player.Nickname} due to: cannot [permissions].");
                        continue;
                    }

                    foreach (RoleTypeId RoleType in Role.SpawnSettings.CanReplaceRoles)
                    {
                        for (int a = 0; a < Role.SpawnSettings.SpawnChance; a++)
                        {
                            RolePercentage[RoleType].Add(Role);
                        }
                    }
                }
            }

            if (Plugin.PlayerRegistry.ContainsKey(Player.Id))
            {
                LogManager.Debug("Was evalutating role select for an already custom role player, stopping");
                return null;
            }

            if (RolePercentage.ContainsKey(Player.Role.Type))
            {
                // We can proceed with the chance
                int Chance = new System.Random().Next(0, 100);
                if (Chance < RolePercentage[Player.Role.Type].Count())
                {
                    // The role exists, good, let's give the player a role
                    int RoleId = RolePercentage[Player.Role.Type].RandomItem().Id;

                    if (Plugin.RolesCount[RoleId].Count() <= Plugin.CustomRoles[RoleId].SpawnSettings.MaxPlayers)
                        return Plugin.CustomRoles[RoleId];
                    else
                        LogManager.Debug($"Player {Player.Nickname} won't be spawned as CustomRole {RoleId} because it has reached the maximus number");
                }
            }

            return null;
        }
    }
}