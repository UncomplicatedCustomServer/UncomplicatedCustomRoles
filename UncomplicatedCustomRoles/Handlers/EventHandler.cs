using System.Collections.Generic;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Interfaces;
using MEC;
using UncomplicatedCustomRoles.Extensions;
using System;
using UncomplicatedCustomRoles.API.Features;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using PluginAPI.Core;
using PlayerRoles;
using UncomplicatedCustomRoles.Events.Attributes;
using UncomplicatedCustomRoles.Events.Args;

namespace UncomplicatedCustomRoles.Handlers
{
    public class EventHandler
    {
        [PluginEvent(ServerEventType.RoundStart)]
        public void OnRoundStarted()
        {
            Plugin.Instance.DoSpawnBasicRoles = false;

            Timing.CallDelayed(1.5f, () =>
            {
                Plugin.Instance.DoSpawnBasicRoles = true;
            });

            foreach (Player Player in Player.GetPlayers())
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

            if (ev.Effect.name is "SeveredHands" && SummonedCustomRole.TryGet(ev.Player, out SummonedCustomRole Role) && Role.Role.MaxScp330Candies >= Role.Scp330Count)
            {
                LogManager.Debug($"Tried to add the {ev.Effect.name} but was not allowed due to {Role.Scp330Count} <= {Role.Role.MaxScp330Candies}");
                ev.Duration = 0;
            }
        }

        [PluginEvent(ServerEventType.Scp049ResurrectBody)]
        public void OnFinishingRecall(Scp049ResurrectBodyEvent ev)
        {
            ICustomRole Role = SpawnManager.DoEvaluateSpawnForPlayer(ev.Player, RoleTypeId.Scp0492);

            if (Role is not null)
                ev.Player.SetCustomRole(Role);
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void OnDied(PlayerDeathEvent ev) => SpawnManager.ClearCustomTypes(ev.Player);

        [PluginEvent(ServerEventType.RoundEnd)]
        public void OnRoundEnded(RoundEndEvent _) => InfiniteEffect.Terminate();

        [PluginEvent(ServerEventType.PlayerChangeRole)]
        public void OnChangingRole(PlayerChangeRoleEvent ev) => SpawnManager.ClearCustomTypes(ev.Player);

        [PluginEvent(ServerEventType.PlayerSpawn)]
        public void OnSpawning(PlayerSpawnEvent ev)
        {
            if (ev.Player is null)
                return;

            LogManager.Debug("Called SPAWNING event");

            if (Spawn.Spawning.Contains(ev.Player.PlayerId))
                return;

            if (ev.Player.HasCustomRole())
                return;

            if (!Plugin.Instance.DoSpawnBasicRoles)
                return;

            string LogReason = string.Empty;
            if (Plugin.Instance.Config.AllowOnlyNaturalSpawns && !Spawn.SpawnQueue.Contains(ev.Player.PlayerId))
            {
                LogManager.Debug("The player is not in the queue for respawning!");
                return;
            }
            else if (Spawn.SpawnQueue.Contains(ev.Player.PlayerId))
            {
                Spawn.SpawnQueue.Remove(ev.Player.PlayerId);
                LogReason = " [WITH a respawn wave - VANILLA]";
            }

            LogManager.Debug($"Player {ev.Player.Nickname} spawned{LogReason}, going to assign a role if needed!");

            // Let's clear for custom types
            SpawnManager.ClearCustomTypes(ev.Player);

            ICustomRole Role = SpawnManager.DoEvaluateSpawnForPlayer(ev.Player);

            if (Role is not null)
            {
                LogManager.Debug($"Summoning player {ev.Player.Nickname} ({ev.Player.PlayerId}) as {Role.Name} ({Role.Id})");
                SpawnManager.SummonCustomSubclass(ev.Player, Role.Id);
            }

            LogManager.Debug($"Evaluated custom role for player {ev.Player.Nickname} - found: {Role?.Id} ({Role?.Name})");
        }

        [InternalPluginEvent("HurtingPlayer")]
        public static void OnHurting(HurtingEventArgs ev)
        {
            if (ev.Player is not null && ev.Player.TryGetSummonedInstance(out SummonedCustomRole AttackerRole))
            {
                // Let's first check if the thing is allowed!
                if (ev.Player is not null && AttackerRole.Role is CustomRole Role && Role.HasTeam(ev.Player.Team))
                {
                    ev.IsAllowed = false;
                    return;
                }

                ev.DamageHandler.Damage *= AttackerRole.Role.DamageMultiplier;
            }
            else if (ev.Player is not null && ev.Attacker is not null && ev.Player.TryGetSummonedInstance(out SummonedCustomRole PlayerRole))
            {
                if (PlayerRole.Role is CustomRole Role && Role.HasTeam(ev.Attacker.Team))
                {
                    ev.IsAllowed = false;
                    return;
                }
            }
        }

        [InternalPluginEvent("PlayerEscaping")]
        public void OnEscaping(EscapingEventArgs ev)
        {
            LogManager.Debug($"Player {ev.Player.Nickname} triggered the escaping event as {ev.Player.Role}");

            if (ev.Player.TryGetSummonedInstance(out SummonedCustomRole summoned))
            {
                LogManager.Debug($"Player IS a custom role: {summoned.Role.Name}");

                if (!summoned.Role.CanEscape)
                {
                    LogManager.Debug($"Player with the role {summoned.Role.Id} ({summoned.Role.Name}) can't escape, so nuh uh!");
                    ev.IsAllowed = false;
                    return;
                }

                if (summoned.Role.CanEscape && (summoned.Role.RoleAfterEscape is null || summoned.Role.RoleAfterEscape.Length < 2))
                {
                    LogManager.Debug($"Player with the role {summoned.Role.Id} ({summoned.Role.Name}) evaluated for a natural respawn!");
                    ev.IsAllowed = true;
                    return;
                }

                // Try to set the role
                KeyValuePair<bool, object> NewRole = SpawnManager.ParseEscapeRole(summoned.Role.RoleAfterEscape, ev.Player);

                if (NewRole.Value is null)
                {
                    ev.IsAllowed = true;
                    return;
                }

                if (!NewRole.Key)
                {
                    // Natural role, let's try to parse it
                    if (Enum.TryParse(NewRole.Value.ToString(), out RoleTypeId role)) 
                    {
                        if (role is not RoleTypeId.None)
                        {
                            ev.IsAllowed = true;
                            ev.NewRole = role;
                        }
                    }
                } 
                else
                {
                    if (int.TryParse(NewRole.Value.ToString(), out int id) && CustomRole.TryGet(id, out ICustomRole role))
                    {
                        ev.IsAllowed = false;
                        SpawnManager.SummonCustomSubclass(ev.Player, role.Id);
                    }

                }
            }
        }

        [InternalPluginEvent("RespawningTeam")]
        public void OnRespawningWave(RespawningTeamEventArgs Respawn)
        {
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