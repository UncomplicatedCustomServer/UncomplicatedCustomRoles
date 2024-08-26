using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.API.Interfaces;
using MEC;
using PlayerRoles;
using UncomplicatedCustomRoles.Extensions;
using System;
using UncomplicatedCustomRoles.API.Features;
using CustomPlayerEffects;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Core;
using PluginAPI.Events;
using UncomplicatedCustomRoles.Events.Attributes;
using UncomplicatedCustomRoles.Events.Args;
using UncomplicatedCustomRoles.Events.Enums;

namespace UncomplicatedCustomRoles.Handlers
{
    internal class EventHandler
    {
        [PluginEvent(ServerEventType.RoundStart)]
        public void OnRoundStarted()
        {
            Plugin.Instance.DoSpawnBasicRoles = false;

            Timing.CallDelayed(1.5f, () =>
            {
                Plugin.Instance.DoSpawnBasicRoles = true;
            });

            foreach (Player Player in Player.GetPlayers().Where(player => player is not null))
            {
                ICustomRole Role = SpawnManager.DoEvaluateSpawnForPlayer(Player);

                LogManager.Debug($"Evaluating role spawn for player {Player.Nickname}, found {Role?.Id} {Role?.Name}!");

                if (Role is not null)
                    SpawnManager.SummonCustomSubclass(Player, Role.Id);
            }
        }

        [PluginEvent(ServerEventType.PlayerInteractScp330)]
        public void OnInteractingScp330(PlayerInteractScp330Event ev)
        {
            if (!ev.AllowPunishment)
                return;

            if (SummonedCustomRole.TryGet(ev.Player, out SummonedCustomRole role))
            {
                role.Scp330Count++;

                LogManager.Debug($"Player {ev.Player} took {role.Scp330Count} candies!");
            }
        }

        [PluginEvent(ServerEventType.PlayerReceiveEffect)]
        public void OnReceivingEffect(PlayerReceiveEffectEvent ev)
        {
            if (ev.Player is null)
                return;

            if (ev.Player.TryGetSummonedInstance(out SummonedCustomRole role))
                if (ev.Effect is SeveredHands && role.Role.MaxScp330Candies >= role.Scp330Count)
                {
                    LogManager.Debug($"Tried to add the {ev.Effect.name} but was not allowed due to {role.Scp330Count} <= {role.Role.MaxScp330Candies}");
                    ev.Player.EffectsManager.DisableEffect<SeveredHands>();
                }
                else if (ev.Effect is CardiacArrest && role.Role.IsFriendOf is not null && role.Role.IsFriendOf.Contains(Team.SCPs))
                    ev.Player.EffectsManager.DisableEffect<CardiacArrest>();
        }

        [PluginEvent(ServerEventType.Scp049ResurrectBody)]
        public void OnFinishingRecall(Scp049ResurrectBodyEvent ev)
        {
            ICustomRole Role = SpawnManager.DoEvaluateSpawnForPlayer(ev.Target, RoleTypeId.Scp0492);

            if (Role is not null)
                ev.Target.SetCustomRole(Role);
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void OnDied(PlayerDeathEvent ev) => SpawnManager.ClearCustomTypes(ev.Player);

        [PluginEvent(ServerEventType.RoundEnd)]
        public void OnRoundEnded(RoundEndEvent _) => InfiniteEffect.Terminate();

        [InternalPluginEvent(EventName.ChangingRole)]
        public void OnSpawning(ChangingRoleEventArgs ev)
        {
            if (ev.Hub is null)
                return;

            if (!ev.IsAllowed)

            SpawnManager.ClearCustomTypes(ev.Hub);

            LogManager.Debug("Called SPAWNING event");

            if (Spawn.Spawning.Contains(ev.Hub.PlayerId))
                return;

            if (SummonedCustomRole.TryGet(ev.Hub, out _))
                return;

            if (!Plugin.Instance.DoSpawnBasicRoles)
                return;

            string LogReason = string.Empty;
            if (Plugin.Instance.Config.AllowOnlyNaturalSpawns && !Spawn.SpawnQueue.Contains(ev.Hub.PlayerId))
            {
                LogManager.Debug("The player is not in the queue for respawning!");
                return;
            }
            else if (Spawn.SpawnQueue.Contains(ev.Hub.PlayerId))
            {
                Spawn.SpawnQueue.Remove(ev.Hub.PlayerId);
                LogReason = " [WITH a respawn wave - VANILLA]";
            }

            LogManager.Debug($"Player {ev.Hub.nicknameSync.Network_myNickSync} spawned{LogReason}, going to assign a role if needed!");

            ICustomRole Role = SpawnManager.DoEvaluateSpawnForPlayer(ev.Hub, ev.NewRole);

            if (Role is not null)
            {
                LogManager.Debug($"Summoning player {ev.Hub.nicknameSync.Network_myNickSync} ({ev.Hub.PlayerId}) as {Role.Name} ({Role.Id})");
                SpawnManager.SummonCustomSubclass(Player.Get(ev.Hub), Role.Id);
            }

            LogManager.Debug($"Evaluated custom role for player {ev.Hub.nicknameSync.Network_myNickSync} - found: {Role?.Id} ({Role?.Name})");
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        public void OnVerified(PlayerJoinedEvent ev) => Plugin.HttpManager.ApplyCreditTag(ev.Player);

        [InternalPluginEvent(EventName.PlayerHurting)]
        public void OnHurting(HurtingEventArgs Hurting)
        {
            LogManager.Silent($"DamageHandler of Hurting: {Hurting.Player} {Hurting.Attacker}");

            if (!Hurting.IsAllowed)
                return;

            if (Hurting.Player is not null && Hurting.Attacker is not null && Hurting.Player.IsAlive && Hurting.Attacker.IsAlive)
            {
                // If the attacker is a custom role we don't check for damage_multiplier but only the thing to avoid -> is_friend_of
                if (Hurting.Attacker.TryGetSummonedInstance(out SummonedCustomRole attackerCustomRole) && attackerCustomRole.Role.IsFriendOf is not null && attackerCustomRole.Role.IsFriendOf.Contains(Hurting.Player.Team))
                {
                    Hurting.IsAllowed = false;
                    LogManager.Silent("Rejected the event request of Hurting because of is_friend_of - A");
                }
                else if (Hurting.Player.TryGetSummonedInstance(out SummonedCustomRole playerCustomRole))
                    if (playerCustomRole.Role.IsFriendOf is not null && playerCustomRole.Role.IsFriendOf.Contains(Hurting.Attacker.Team))
                    {
                        Hurting.IsAllowed = false;
                        LogManager.Silent("Rejected the event request of Hurting because of is_friend_of - B");
                        return;
                    }
                    else
                        Hurting.DamageHandler.Damage *= playerCustomRole.Role.DamageMultiplier;
            }
        }

        [InternalPluginEvent(EventName.PlayerEscaping)]
        public void OnEscaping(EscapingEventArgs Escaping)
        {
            LogManager.Debug($"Player {Escaping.Player.Nickname} triggered the escaping event as {Escaping.Player.Role}");

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
                        if (!API.Features.Escape.Bucket.Contains(Escaping.Player.PlayerId))
                        {
                            LogManager.Silent($"Successfully activated the call to method SpawnManager::SummonCustomSubclass(<...>) as the player is not inside the Escape::Bucket bucket! - Adding it...");
                            API.Features.Escape.Bucket.Add(Escaping.Player.PlayerId);
                            SpawnManager.SummonCustomSubclass(Escaping.Player, role.Id);
                        }
                        else
                            LogManager.Silent($"Canceled call to method SpawnManager::SummonCustomSubclass(<...>) due to the presence of the player inside the Escape::Bucket! - Event already fired!");
                    }

                }
            }
        }

        [InternalPluginEvent(EventName.RespawningTeam)]
        public void OnRespawningWave(RespawningTeamEventArgs Respawn)
        {
            LogManager.Silent("Respawning wave");
            if (Spawn.DoHandleWave)
                foreach (ReferenceHub Player in Respawn.RespawnQueue)
                    Spawn.SpawnQueue.Add(Player.PlayerId);
            else
                Spawn.DoHandleWave = true;
        }

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        public void OnItemUsed(PlayerUsedItemEvent UsedItem)
        {
            if (UsedItem.Player is not null && UsedItem.Player.TryGetSummonedInstance(out SummonedCustomRole summoned) && UsedItem.Item.ItemTypeId == ItemType.SCP500)
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