/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using PlayerStatsSystem;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Integrations;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UncomplicatedCustomRoles.Events;

internal class PlayerEventHandler : EventHandlerBase
{
    private static Scp079Recontainer _recontainer;
    internal static PlayerEventHandler Instance { get; private set; }

    internal override void OnRegistered()
    {
        PlayerEvents.ActivatingGenerator += OnGenerator;
        PlayerEvents.Dying += OnDying;
        PlayerEvents.Death += OnDeath;
        PlayerEvents.SpawningRagdoll += OnRagdollSpawn;
        PlayerEvents.ChangingRole += OnChangingRole;
        PlayerEvents.UpdatingEffect += OnUpdatingEffect;
        PlayerEvents.Escaping += OnEscaping;
        PlayerEvents.UsedItem += OnItemUsed;
        PlayerEvents.Hurting += OnHurting;
        PlayerEvents.Hurt += OnHurt;
        PlayerEvents.PickingUpItem += OnPickingUpItem;
        PlayerEvents.Joined += OnJoined;
        PlayerEvents.DamagingWindow += OnDamagingWindow;
        PlayerEvents.UnlockingWarheadButton += OnUnlockingWarheadButton;
        PlayerEvents.RequestedRaPlayerInfo += OnPlayerRequestedRaPlayerInfo;
        PlayerEvents.RaPlayerListAddingPlayer += OnPlayerRaPlayerListAddingPlayer;
        PlayerEvents.ChangedNickname += OnChangedNickname;
        PlayerEvents.PickingUpArmor += OnPickingUpArmor;
        PlayerEvents.PickingUpScp330 += OnPickingUpScp330;
        PlayerEvents.InteractingScp330 += OnInteractingScp330;

        Instance = this;
    }

    internal override void OnUnregistered()
    {
        Instance = null;

        PlayerEvents.ActivatingGenerator -= OnGenerator;
        PlayerEvents.Dying -= OnDying;
        PlayerEvents.Death -= OnDeath;
        PlayerEvents.SpawningRagdoll -= OnRagdollSpawn;
        PlayerEvents.ChangingRole -= OnChangingRole;
        PlayerEvents.UpdatingEffect -= OnUpdatingEffect;
        PlayerEvents.Escaping -= OnEscaping;
        PlayerEvents.UsedItem -= OnItemUsed;
        PlayerEvents.Hurting -= OnHurting;
        PlayerEvents.Hurt -= OnHurt;
        PlayerEvents.PickingUpItem -= OnPickingUpItem;
        PlayerEvents.Joined -= OnJoined;
        PlayerEvents.DamagingWindow -= OnDamagingWindow;
        PlayerEvents.UnlockingWarheadButton -= OnUnlockingWarheadButton;
        PlayerEvents.RequestedRaPlayerInfo -= OnPlayerRequestedRaPlayerInfo;
        PlayerEvents.RaPlayerListAddingPlayer -= OnPlayerRaPlayerListAddingPlayer;
        PlayerEvents.ChangedNickname -= OnChangedNickname;
        PlayerEvents.PickingUpArmor -= OnPickingUpArmor;
        PlayerEvents.PickingUpScp330 -= OnPickingUpScp330;
        PlayerEvents.InteractingScp330 -= OnInteractingScp330;
    }

    public void OnJoined(PlayerJoinedEventArgs ev)
    {
        FirstRoundPlayers.Add(ev.Player.PlayerId);

        // Sync role appearance - LabApiExtensions handles it if available
        if (!LabApiExtensions.IsAvailable)
            foreach (var role in SummonedCustomRole.List.Values.Where(role => role.Appearance != RoleTypeId.None))
                role.Player.ChangeAppearance(role.Appearance, [ev.Player]);

        foreach (var role in SummonedCustomRole.List.Values.Where(role => role.Scale != Vector3.one))
            role.Player.Scale = role.Scale;
    }

    public void OnUpdatingEffect(PlayerEffectUpdatingEventArgs ev)
    {
        if (ev.Player is null)
            return;

        if (!ev.IsAllowed)
            return;

        if (ev.Player.TryGetSummonedInstance(out var role))
            switch (ev.Effect)
            {
                case CardiacArrest when role.Role.IsFriendOf is not null && role.Role.IsFriendOf.Contains(Team.SCPs):
                case AmnesiaVision or AmnesiaItems when role.HasModule<AmnesiaResistance>():
                    ev.IsAllowed = false;
                    break;
            }
    }

    public void OnGenerator(PlayerActivatingGeneratorEventArgs ev)
    {
        if (SummonedCustomRole.TryGetCustomTeam(ev.Player.ReferenceHub) == Team.SCPs)
            ev.IsAllowed = false;
    }

    public void OnUnlockingWarheadButton(PlayerUnlockingWarheadButtonEventArgs ev)
    {
        if (SummonedCustomRole.TryGetCustomTeam(ev.Player.ReferenceHub) == Team.SCPs)
            ev.IsAllowed = false;
    }

    public void OnDamagingWindow(PlayerDamagingWindowEventArgs ev)
    {
        if (SummonedCustomRole.TryGetCustomTeam(ev.Player.ReferenceHub) != Team.SCPs)
            return;

        if (_recontainer == null)
            _recontainer = Object.FindAnyObjectByType<Scp079Recontainer>();

        if (_recontainer != null && ev.Window.name == _recontainer._activatorGlass.name)
            ev.IsAllowed = false;
    }

    public void OnDying(PlayerDyingEventArgs ev)
    {
        if (ev.Player.TryGetSummonedInstance(out var customRole))
        {
            if (customRole.HasModule<TutorialRagdoll>())
                RagdollAppearanceQueue.Add(ev.Player.PlayerId);

            if (customRole.TryGetModule(out CustomScpAnnouncer announcer) &&
                ev.Player.ReferenceHub.GetTeam() is not Team.SCPs)
                TerminationQueue[ev.Player.PlayerId] =
                    new Tuple<CustomScpAnnouncer, DateTimeOffset>(announcer, DateTimeOffset.Now);

            if (customRole.HasModule<DropNothingOnDeath>())
                ev.Player.ClearInventory();
        }
    }

    public void OnDeath(PlayerDeathEventArgs ev)
    {
        if (TerminationQueue.TryGetValue(ev.Player.PlayerId, out var data) &&
            (DateTimeOffset.Now - data.Item2).TotalMilliseconds < 1300)
            SpawnManager.AnnounceScpTermination(ev.Player.ReferenceHub, ev.DamageHandler);

        TerminationQueue.TryRemove(ev.Player.PlayerId, out _);

        SpawnManager.ClearCustomTypes(ev.Player);

        // Try change appearance of the killer
        if (ev.Attacker.TryGetSummonedInstance(out var attackerCustomRole) &&
            attackerCustomRole.TryGetModule(out ChangeAppearanceOnKill changeAppearanceOnKill))
        {
            if (changeAppearanceOnKill.Forever && changeAppearanceOnKill.AlreadyChanged)
                return;

            changeAppearanceOnKill.AlreadyChanged = true;

            // Change
            if (LabApiExtensions.IsAvailable)
                LabApiExtensions.AddFakeRole(attackerCustomRole.Player, changeAppearanceOnKill.NewAppearance);
            else
                attackerCustomRole.Player.ChangeAppearance(changeAppearanceOnKill.NewAppearance);

            if (!changeAppearanceOnKill.Forever)
                Timing.CallDelayed(changeAppearanceOnKill.Duration, () =>
                {
                    if (attackerCustomRole.Player is null || !attackerCustomRole.Player.IsAlive)
                        return;

                    if (LabApiExtensions.IsAvailable)
                    {
                        if (attackerCustomRole.Appearance != RoleTypeId.None)
                            LabApiExtensions.AddFakeRole(attackerCustomRole.Player,
                                attackerCustomRole.Role.RoleAppearance);
                        else
                            LabApiExtensions.RemoveFakeRole(attackerCustomRole.Player);
                    }
                    else
                    {
                        attackerCustomRole.Player.ChangeAppearance(attackerCustomRole.Role.RoleAppearance);
                    }
                });
        }

        // DON'T DO ANYTHING HERE AS THERE ARE TWO return UP THERE!
    }

    public void OnRagdollSpawn(PlayerSpawningRagdollEventArgs ev)
    {
        if (ev.Player is null)
            return;

        if (!RagdollAppearanceQueue.Contains(ev.Player.PlayerId))
            return;

        ev.IsAllowed = false;
        RagdollAppearanceQueue.Remove(ev.Player.PlayerId);

        Ragdoll.SpawnRagdoll(RoleTypeId.Tutorial, ev.RagdollPrefab.Position, ev.RagdollPrefab.Rotation,
            ev.DamageHandler, ev.Player.Nickname);
    }

    public void OnChangingRole(PlayerChangingRoleEventArgs ev)
    {
        if (ev.Player is null)
            return;

        // Let's clear for custom types
        SpawnManager.ClearCustomTypes(ev.Player);

        if (!ev.IsAllowed)
            return;

        if (!Round.IsRoundStarted)
            return;

        if (ev.NewRole is RoleTypeId.Spectator or RoleTypeId.None or RoleTypeId.Filmmaker)
            return;

        if (Spawn.Spawning.Contains(ev.Player.PlayerId))
            return;

        if (ev.Player.HasCustomRole())
            return;

        if (Plugin.Instance.Config.IgnoreNpcs && ev.Player.IsNpc)
            return;

        if (Started || !FirstRoundPlayers.Contains(ev.Player.PlayerId))
        {
            if (Plugin.Instance.Config.AllowOnlyNaturalSpawns && !Spawn.SpawnQueue.Contains(ev.Player.PlayerId))
            {
                LogManager.Debug("The player is not in the queue for respawning!");
                return;
            }

            if (Spawn.SpawnQueue.Contains(ev.Player.PlayerId)) Spawn.SpawnQueue.Remove(ev.Player.PlayerId);
        }

        var Role = SpawnManager.DoEvaluateSpawnForPlayer(ev.Player, ev.NewRole);

        if (Role is not null)
        {
            LogManager.Debug(
                $"Summoning player {ev.Player.Nickname} ({ev.Player.PlayerId}) as {Role.Name} ({Role.Id})");
            SpawnManager.SummonCustomSubclass(ev.Player, Role.Id);
            ev.IsAllowed = false;
        }
        else
        {
            LogManager.Debug(
                $"No CustomRole found for player {ev.Player.Nickname}, allowing natural spawn with {ev.NewRole}");
        }
    }

    public void OnHurting(PlayerHurtingEventArgs Hurting)
    {
        if (!Hurting.IsAllowed)
            return;

        if (Hurting.Player is not null && Hurting.Attacker is not null && Hurting.Player.IsAlive &&
            Hurting.Attacker.IsAlive)
        {
            if (Hurting.Attacker.TryGetSummonedInstance(out var attackerCustomRole))
            {
                if (attackerCustomRole.Role.IsFriendOf is not null &&
                    attackerCustomRole.Role.IsFriendOf.Contains(Hurting.Player.ReferenceHub.GetTeam()))
                {
                    Hurting.IsAllowed = false;
                    LogManager.Silent("Rejected the event request of Hurting because of is_friend_of - FROM ATTACKER");
                    return;
                }

                if (attackerCustomRole?.HasModule<PacifismUntilDamage>() ?? false)
                    attackerCustomRole.RemoveModules<PacifismUntilDamage>();

                if (Hurting.DamageHandler is StandardDamageHandler standardDamageHandler)
                    standardDamageHandler.Damage *= attackerCustomRole.Role.DamageMultiplier;
            }

            // Divided because they can be both CR
            if (Hurting.Player.TryGetSummonedInstance(out var playerCustomRole))
            {
                if (playerCustomRole.Role.IsFriendOf is not null &&
                    playerCustomRole.Role.IsFriendOf.Contains(Hurting.Attacker.ReferenceHub.GetTeam()))
                {
                    Hurting.IsAllowed = false;
                    LogManager.Silent("Rejected the event request of Hurting because of is_friend_of - FROM HURTED");
                    return;
                }

                if (playerCustomRole?.HasModule<PacifismUntilDamage>() ?? false)
                    Hurting.IsAllowed = false;
            }
        }
    }

    public void OnHurt(PlayerHurtEventArgs ev)
    {
        if (ev.Player is not null && ev.Player.IsAlive && ev.Player.TryGetSummonedInstance(out var playerCustomRole))
            playerCustomRole.LastDamageTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (ev.Attacker is not null && ev.Attacker.IsAlive &&
            ev.Attacker.TryGetSummonedInstance(out var attackerCustomRole) &&
            attackerCustomRole.TryGetModule(out LifeStealer lifeStealer) &&
            ev.DamageHandler is StandardDamageHandler standardDamageHandler)
            ev.Attacker.Heal(standardDamageHandler.Damage * (lifeStealer.Percentage / 100f));
    }

    public void OnEscaping(PlayerEscapingEventArgs Escaping)
    {
        if (Escaping.Player.TryGetSummonedInstance(out var summoned))
        {
            if (summoned.Role.CanEscape)
                LogManager.Debug(
                    $"Player {Escaping.Player.Nickname} triggered the escaping event as {Escaping.Player.Role.ToString()}");

            LogManager.Debug($"Player IS a custom role: {summoned.Role.Name}");

            if (!summoned.Role.CanEscape)
            {
                LogManager.Debug(
                    $"Player with the role {summoned.Role.Id} ({summoned.Role.Name}) can't escape, so nuh uh!");
                Escaping.IsAllowed = false;
                return;
            }

            if (summoned.Role.CanEscape &&
                (summoned.Role.RoleAfterEscape is null || summoned.Role.RoleAfterEscape.Count < 1))
            {
                LogManager.Debug(
                    $"Player with the role {summoned.Role.Id} ({summoned.Role.Name}) evaluated for a natural respawn!");
                Escaping.IsAllowed = true;
                return;
            }

            // Try to set the role
            var newRole = SpawnManager.ParseEscapeRole(summoned.Role.RoleAfterEscape, Escaping.Player);

            if (newRole is null)
            {
                Escaping.IsAllowed = false;
                return;
            }

            var NewRole = (KeyValuePair<bool, object>)newRole;

            if (NewRole.Value is null)
            {
                Escaping.IsAllowed = true;
                return;
            }

            if (!NewRole.Key)
            {
                // Natural role, let's try to parse it
                if (Enum.TryParse(NewRole.Value.ToString(), out RoleTypeId role))
                    if (role is not RoleTypeId.None)
                    {
                        Escaping.NewRole = role;
                        Escaping.IsAllowed = true;
                    }
            }
            else
            {
                LogManager.Silent($"Trying to find CustomRole with Id {NewRole.Value}");
                if (int.TryParse(NewRole.Value.ToString(), out var id) && CustomRole.TryGet(id, out var role))
                {
                    LogManager.Silent("Role found!");

                    if (summoned.TryGetModule(out KeepInventoryOnEscape module))
                        RespawnInventoryQueue.TryAdd(Escaping.Player.PlayerId,
                            new Tuple<List<ItemType>, Dictionary<ItemType, ushort>, bool>(
                                [..Escaping.Player.Items.Select(i => i.Type)],
                                new Dictionary<ItemType, ushort>(Escaping.Player.Ammo), module.DropItems));

                    Escaping.IsAllowed = false;
                    if (!API.Features.Escape.Bucket.Contains(Escaping.Player.PlayerId))
                    {
                        LogManager.Silent(
                            "Successfully activated the call to method SpawnManager::SummonCustomSubclass(<...>) as the player is not inside the Escape::Bucket bucket! - Adding it...");
                        API.Features.Escape.Bucket.Add(Escaping.Player.PlayerId);
                        SpawnManager.SummonCustomSubclass(Escaping.Player, role.Id);
                    }
                    else
                    {
                        LogManager.Silent(
                            "Canceled call to method SpawnManager::SummonCustomSubclass(<...>) due to the presence of the player inside the Escape::Bucket! - Event already fired!");
                    }
                }
            }
        }
    }

    public void OnItemUsed(PlayerUsedItemEventArgs ev)
    {
        if (ev.Player is not null && ev.Player.TryGetSummonedInstance(out var summoned) &&
            ev.UsableItem.Type is ItemType.SCP500)
            summoned?.InfiniteEffects.RemoveAll(effect => effect is not null && effect.Removable);
    }

    public void OnPickingUpItem(PlayerPickingUpItemEventArgs ev)
    {
        if (ev.Player.TryGetSummonedInstance(out var summonedInstance) &&
            summonedInstance.TryGetModule(out ItemBan itemBan))
            ev.IsAllowed = !itemBan.Items.Contains(ev.Pickup.Type);
    }

    public void OnPickingUpArmor(PlayerPickingUpArmorEventArgs ev)
    {
        if (ev.Player.TryGetSummonedInstance(out var summonedInstance) &&
            summonedInstance.TryGetModule(out ItemBan itemBan))
            ev.IsAllowed = !itemBan.Items.Contains(ev.BodyArmorPickup.Type);
    }

    public void OnPickingUpScp330(PlayerPickingUpScp330EventArgs ev)
    {
        if (ev.Player.TryGetSummonedInstance(out var summonedInstance) &&
            summonedInstance.TryGetModule(out ItemBan itemBan))
            ev.IsAllowed = !itemBan.Items.Contains(ev.CandyPickup.Type);
    }

    public void OnInteractingScp330(PlayerInteractingScp330EventArgs ev)
    {
        if (ev.Player.TryGetSummonedInstance(out var summonedInstance) &&
            summonedInstance.TryGetModule(out ItemBan itemBan))
            ev.IsAllowed = !itemBan.Items.Contains(ItemType.SCP330);
    }

    public void OnPlayerRequestedRaPlayerInfo(PlayerRequestedRaPlayerInfoEventArgs ev)
    {
        SummonedCustomRole.TryParseRemoteAdmin(ev.Target.ReferenceHub, ev.InfoBuilder);
    }

    public void OnPlayerRaPlayerListAddingPlayer(PlayerRaPlayerListAddingPlayerEventArgs ev)
    {
        if (SummonedCustomRole.TryGet(ev.Target.ReferenceHub, out var customRole))
            if (customRole.TryGetModule(out ColorfulRaName colorfulRaName))
                ev.Body = ev.Body.Replace("{RA_ClassColor}", $"#{colorfulRaName.Color.TrimStart('#')}");
    }

    public void OnChangedNickname(PlayerChangedNicknameEventArgs ev)
    {
        if (ev.Player.ReferenceHub is null)
            return;

        if (SummonedCustomRole.TryGet(ev.Player.ReferenceHub, out var customRole) && customRole.CustomInfo is not null)
            customRole.CustomInfo.Nickname = ev.NewNickname ?? ev.Player.Nickname;
    }
}