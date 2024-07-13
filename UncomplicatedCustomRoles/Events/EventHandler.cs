using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Interfaces;
using MEC;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Scp049;
using Exiled.API.Enums;
using UnityEngine;
using UncomplicatedCustomRoles.Extensions;
using System;
using UncomplicatedCustomRoles.API.Features;
using Exiled.Events.EventArgs.Scp330;
using UncomplicatedCustomRoles.API.Features.Behaviour;

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
                Plugin.RolesCount[Data.Key] = new();

            Plugin.Instance.DoSpawnBasicRoles = false;

            if (EffectCoroutine.IsRunning)
                Timing.KillCoroutines(EffectCoroutine);

            EffectCoroutine = Timing.RunCoroutine(DoSetInfiniteEffectToPlayers());

            Timing.CallDelayed(0.75f, () =>
            {
                Plugin.Instance.DoSpawnBasicRoles = true;
            });

            foreach (Player Player in Player.List.Where(player => !player.IsNPC))
            {
                ICustomRole Role = SpawnManager.DoEvaluateSpawnForPlayer(Player);

                LogManager.Debug($"Evaluating role spawn for player {Player.Nickname}, found {Role?.Id} {Role?.Name}!");

                if (Role is not null)
                    SpawnManager.SummonCustomSubclass(Player, Role.Id);
            }
        }

        public void OnInteractingScp330(InteractingScp330EventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            Plugin.Scp330Count.TryAdd(ev.Player.Id, Plugin.Scp330Count.TryGetElement<int, uint>(ev.Player.Id, 0) + 1);

            LogManager.Debug($"Player {ev.Player} eaten {Plugin.Scp330Count[ev.Player.Id]} candies!");
        }

        public void OnReceivingEffect(ReceivingEffectEventArgs ev)
        {
            if (ev.Player is null)
                return;

            if (!ev.IsAllowed)
                return;

            if (ev.Effect.name is "SeveredHands" && ev.Player.TryGetCustomRole(out ICustomRole Role) && Role.MaxScp330Candies >= Plugin.Scp330Count.TryGetElement<int, uint>(ev.Player.Id, 0))
            {
                LogManager.Debug($"Tried to add the {ev.Effect.name} but was not allowed due to {Plugin.Scp330Count.TryGetElement<int, uint>(ev.Player.Id, 0)} <= {Role.MaxScp330Candies}");
                ev.IsAllowed = false;
            }
        }

        public void OnPlayerSpawned(SpawnedEventArgs Spawned)
        {
            if (Spawned.Player.HasCustomRole() && Plugin.InternalCooldownQueue.Contains(Spawned.Player.Id))
                Plugin.InternalCooldownQueue.Remove(Spawned.Player.Id);
        }

        public void OnScp049StartReviving(StartingRecallEventArgs Recall)
        {
            if (Plugin.CustomRoles.Where(cr => cr.Value.SpawnSettings is not null && cr.Value.SpawnSettings.CanReplaceRoles.Contains(RoleTypeId.Scp0492)).Count() > 0)
                Plugin.RoleSpawnQueue.Add(Recall.Target.Id);
        }

        public void OnDied(DiedEventArgs Died) => SpawnManager.ClearCustomTypes(Died.Player);

        public void OnChangingRole(ChangingRoleEventArgs ev) => SpawnManager.ClearCustomTypes(ev.Player);

        public void OnSpawning(SpawningEventArgs ev)
        {
            if (ev.Player is null)
                return;

            LogManager.Debug("Called CHANGINGROLE event");

            if (Plugin.InternalCooldownQueue.Contains(ev.Player.Id))
                return;

            LogManager.Debug("Called CHANGINGROLE A event");

            if (Plugin.PlayerRegistry.ContainsKey(ev.Player.Id))
                return;

            LogManager.Debug("Called CHANGINGROLE B event");

            if (!Plugin.Instance.DoSpawnBasicRoles)
                return;

            LogManager.Debug("Called CHANGINGROLE C event");

            if (ev.Player.IsNPC)
                return;

            LogManager.Debug("Called CHANGINGROLE E event");

            string LogReason = string.Empty;
            if (Plugin.Instance.Config.AllowOnlyNaturalSpawns && !Plugin.RoleSpawnQueue.Contains(ev.Player.Id))
            {
                LogManager.Debug("The player is not in the queue for respawning!");
                return;
            }
            else if (Plugin.RoleSpawnQueue.Contains(ev.Player.Id))
            {
                Plugin.RoleSpawnQueue.Remove(ev.Player.Id);
                LogReason = " [going with a respawn wave OR 049 revival]";
            }

            LogManager.Debug($"Player {ev.Player.Nickname} spawned{LogReason}, going to assign a role if needed!");

            // Let's clear for custom types
            SpawnManager.ClearCustomTypes(ev.Player);

            ICustomRole Role = SpawnManager.DoEvaluateSpawnForPlayer(ev.Player);

            if (Role is not null)
            {
                LogManager.Debug($"Summoning player {ev.Player.Nickname} ({ev.Player.Id}) as {Role.Name} ({Role.Id})");
                SpawnManager.SummonCustomSubclass(ev.Player, Role.Id);
            }

            LogManager.Debug($"Evaluated custom role for player {ev.Player.Nickname} - found: {Role?.Id} ({Role?.Name})");
        }

        public void OnHurting(HurtingEventArgs Hurting)
        {
            if (Hurting.Attacker is not null && Hurting.Attacker.TryGetCustomRole(out ICustomRole AttackerRole))
            {
                // Let's first check if te thing is allowed!
                if (Hurting.Player is not null && AttackerRole is Elements.CustomRole CustomRole && CustomRole.HasTeam(Hurting.Player.Role.Team))
                {
                    Hurting.IsAllowed = false;
                    return;
                }

                Hurting.DamageHandler.Damage *= AttackerRole.DamageMultiplier;
            }
            else if (Hurting.Player is not null && Hurting.Attacker is not null && Hurting.Player.TryGetCustomRole(out ICustomRole PlayerRole))
            {
                if (PlayerRole is Elements.CustomRole CustomRole && CustomRole.HasTeam(Hurting.Attacker.Role.Team))
                {
                    Hurting.IsAllowed = false;
                    return;
                }
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
                KeyValuePair<bool, object> NewRole = SpawnManager.ParseEscapeRole(Role.RoleAfterEscape, Escaping.Player);

                if (NewRole.Value is null)
                {
                    Escaping.IsAllowed = true;
                    return;
                }

                if (!NewRole.Key)
                {
                    // Natural role, let's try to parse it
                    if (Enum.TryParse(NewRole.Value.ToString(), out RoleTypeId role)) 
                    {
                        if (role is not RoleTypeId.None)
                        {
                            Escaping.IsAllowed = true;
                            Escaping.NewRole = role;
                        }
                    }
                } 
                else
                {
                    if (int.TryParse(NewRole.Value.ToString(), out int id) && CustomRole.TryGet(id, out ICustomRole role))
                    {
                        Escaping.IsAllowed = false;
                        SpawnManager.SummonCustomSubclass(Escaping.Player, role.Id);
                    }

                }
            }
        }

        public void OnRespawningWave(RespawningTeamEventArgs Respawn)
        {
            if (SpawnBehaviour.DoHandleWave)
                foreach (Player Player in Respawn.Players)
                    Plugin.RoleSpawnQueue.Add(Player.Id);
            else
                SpawnBehaviour.DoHandleWave = true;
        }

        public void OnItemUsed(UsedItemEventArgs UsedItem)
        {
            if (UsedItem.Player is not null && UsedItem.Player.HasCustomRole() && Plugin.PermanentEffectStatus.ContainsKey(UsedItem.Player.Id) && UsedItem.Item.Type == ItemType.SCP500)
            {
                foreach (IUCREffect Effect in Plugin.PermanentEffectStatus[UsedItem.Player.Id])
                    if (Effect.Removable)
                        Plugin.PermanentEffectStatus[UsedItem.Player.Id].Remove(Effect);
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
    }
}