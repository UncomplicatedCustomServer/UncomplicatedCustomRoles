using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Manager;
using MEC;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Scp049;
using UncomplicatedCustomRoles.Extensions;
using System;
using UncomplicatedCustomRoles.API.Features;
using Exiled.Events.EventArgs.Scp330;
using CustomPlayerEffects;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Events
{
    internal class EventHandler
    {
        public void OnRoundStarted()
        {
            Plugin.Instance.DoSpawnBasicRoles = false;

            Timing.CallDelayed(1.5f, () =>
            {
                Plugin.Instance.DoSpawnBasicRoles = true;
            });

            foreach (Player Player in Player.List.Where(player => !player.IsNPC && player is not null))
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

            if (SummonedCustomRole.TryGet(ev.Player, out SummonedCustomRole role))
            {
                role.Scp330Count++;

                LogManager.Debug($"Player {ev.Player} took {role.Scp330Count} candies!");
            }
        }

        public void OnReceivingEffect(ReceivingEffectEventArgs ev)
        {
            if (ev.Player is null)
                return;

            if (!ev.IsAllowed)
                return;

            if (ev.Player.TryGetSummonedInstance(out SummonedCustomRole role))
                if (ev.Effect is SeveredHands && role.Role.MaxScp330Candies >= role.Scp330Count)
                {
                    LogManager.Debug($"Tried to add the {ev.Effect.name} but was not allowed due to {role.Scp330Count} <= {role.Role.MaxScp330Candies}");
                    ev.IsAllowed = false;
                }
                else if (ev.Effect is CardiacArrest && role.Role.IsFriendOf is not null && role.Role.IsFriendOf.Contains(Team.SCPs))
                    ev.IsAllowed = false;
        }

        public void OnPlayerSpawned(SpawnedEventArgs _) { }

        public void OnFinishingRecall(FinishingRecallEventArgs ev)
        {
            ICustomRole Role = SpawnManager.DoEvaluateSpawnForPlayer(ev.Target, RoleTypeId.Scp0492);

            if (Role is not null)
            {
                ev.IsAllowed = false;
                ev.Target.SetCustomRole(Role);
            }
        }

        public void OnDied(DiedEventArgs ev) => SpawnManager.ClearCustomTypes(ev.Player);

        public void OnRoundEnded(RoundEndedEventArgs _) => InfiniteEffect.Terminate();

        public void OnChangingRole(ChangingRoleEventArgs ev) => SpawnManager.ClearCustomTypes(ev.Player);

        public void OnSpawning(SpawningEventArgs ev)
        {
            if (ev.Player is null)
                return;

            LogManager.Debug("Called SPAWNING event");

            if (Spawn.Spawning.Contains(ev.Player.Id))
                return;

            if (ev.Player.HasCustomRole())
                return;

            if (!Plugin.Instance.DoSpawnBasicRoles)
                return;

            if (ev.Player.IsNPC)
                return;

            string LogReason = string.Empty;
            if (Plugin.Instance.Config.AllowOnlyNaturalSpawns && !Spawn.SpawnQueue.Contains(ev.Player.Id))
            {
                LogManager.Debug("The player is not in the queue for respawning!");
                return;
            }
            else if (Spawn.SpawnQueue.Contains(ev.Player.Id))
            {
                Spawn.SpawnQueue.Remove(ev.Player.Id);
                LogReason = " [WITH a respawn wave - VANILLA]";
            }

            LogManager.Debug($"Player {ev.Player.Nickname} spawned{LogReason}, going to assign a role if needed!");

            // Let's clear for custom types
            SpawnManager.ClearCustomTypes(ev.Player);

            ICustomRole Role = SpawnManager.DoEvaluateSpawnForPlayer(ev.Player);

            if (Role is not null)
            {
                LogManager.Debug($"Summoning player {ev.Player.Nickname} ({ev.Player.Id}) as {Role.Name} ({Role.Id})");
                Timing.CallDelayed(0.3f, () => SpawnManager.SummonCustomSubclass(ev.Player, Role.Id));
            }

            LogManager.Debug($"Evaluated custom role for player {ev.Player.Nickname} - found: {Role?.Id} ({Role?.Name})");
        }

        public void OnVerified(VerifiedEventArgs ev) => Plugin.HttpManager.ApplyCreditTag(ev.Player);

        public void OnHurting(HurtingEventArgs Hurting)
        {
            if (!Hurting.IsAllowed)
                return;

            LogManager.Silent($"DamageHandler of Hurting: {Hurting.Player} {Hurting.Attacker}");

            if (Hurting.Player is not null && Hurting.Attacker is not null && Hurting.Player.IsAlive && Hurting.Attacker.IsAlive)
            {
                if (Hurting.Attacker.TryGetSummonedInstance(out SummonedCustomRole attackerCustomRole))
                    if (attackerCustomRole.Role.IsFriendOf is not null && attackerCustomRole.Role.IsFriendOf.Contains(Hurting.Player.Role.Team))
                    {
                        Hurting.IsAllowed = false;
                        LogManager.Silent("Rejected the event request of Hurting because of is_friend_of - FROM ATTACKER");
                    }
                    else
                        Hurting.DamageHandler.Damage *= attackerCustomRole.Role.DamageMultiplier;
                else if (Hurting.Player.TryGetSummonedInstance(out SummonedCustomRole playerCustomRole) && playerCustomRole.Role.IsFriendOf is not null && playerCustomRole.Role.IsFriendOf.Contains(Hurting.Attacker.Role.Team))
                {
                    Hurting.IsAllowed = false;
                    LogManager.Silent("Rejected the event request of Hurting because of is_friend_of - FROM HURTED");
                }
            }
        }

        public void OnEscaping(EscapingEventArgs Escaping)
        {
            LogManager.Debug($"Player {Escaping.Player.Nickname} triggered the escaping event as {Escaping.Player.Role.Name}");

            if (Escaping.Player.TryGetSummonedInstance(out SummonedCustomRole summoned))
            {
                LogManager.Debug($"Player IS a custom role: {summoned.Role.Name}");

                if (!summoned.Role.CanEscape)
                {
                    LogManager.Debug($"Player with the role {summoned.Role.Id} ({summoned.Role.Name}) can't escape, so nuh uh!");
                    Escaping.IsAllowed = false;
                    return;
                }

                if (summoned.Role.CanEscape && (summoned.Role.RoleAfterEscape is null || summoned.Role.RoleAfterEscape.Count < 1))
                {
                    LogManager.Debug($"Player with the role {summoned.Role.Id} ({summoned.Role.Name}) evaluated for a natural respawn!");
                    Escaping.IsAllowed = true;
                    return;
                }

                // Try to set the role
                KeyValuePair<bool, object> NewRole = SpawnManager.ParseEscapeRole(summoned.Role.RoleAfterEscape, Escaping.Player);

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
                            Escaping.NewRole = role;
                            Escaping.IsAllowed = true;
                        }
                    }
                } 
                else
                {
                    if (int.TryParse(NewRole.Value.ToString(), out int id) && CustomRole.TryGet(id, out ICustomRole role))
                    {
                        Escaping.IsAllowed = false;
                        if (!API.Features.Escape.Bucket.Contains(Escaping.Player.Id))
                        {
                            LogManager.Silent($"Successfully activated the call to method SpawnManager::SummonCustomSubclass(<...>) as the player is not inside the Escape::Bucket bucket! - Adding it...");
                            API.Features.Escape.Bucket.Add(Escaping.Player.Id);
                            SpawnManager.SummonCustomSubclass(Escaping.Player, role.Id);
                        }
                        else
                            LogManager.Silent($"Canceled call to method SpawnManager::SummonCustomSubclass(<...>) due to the presence of the player inside the Escape::Bucket! - Event already fired!");
                    }

                }
            }
        }

        public void OnRespawningWave(RespawningTeamEventArgs Respawn)
        {
            LogManager.Silent("Respawning wave");
            if (Spawn.DoHandleWave)
                foreach (Player Player in Respawn.Players)
                    Spawn.SpawnQueue.Add(Player.Id);
            else
                Spawn.DoHandleWave = true;
        }

        public void OnItemUsed(UsedItemEventArgs UsedItem)
        {
            if (UsedItem.Player is not null && UsedItem.Player.TryGetSummonedInstance(out SummonedCustomRole summoned) && UsedItem.Item.Type == ItemType.SCP500)
                foreach (IEffect Effect in summoned.InfiniteEffects)
                    if (Effect.Removable)
                        summoned.InfiniteEffects.Remove(Effect);
        }

        public static IEnumerator<float> DoSpawnPlayer(Player Player, int Id, bool DoBypassRoleOverwrite = true)
        {
            yield return Timing.WaitForSeconds(0.1f);
            SpawnManager.SummonCustomSubclass(Player, Id, DoBypassRoleOverwrite);
        }
    }
}