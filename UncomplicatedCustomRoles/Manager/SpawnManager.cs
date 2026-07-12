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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cassie;
using Footprinting;
using InventorySystem;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using Subtitles;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.Controllers;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.API.Struct;
using UncomplicatedCustomRoles.Events;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Integrations;
using UncomplicatedCustomRoles.Patches;
using UnityEngine;
using Random = UnityEngine.Random;

// Mormora, la gente mormora
// falla tacere praticando l'allegria

namespace UncomplicatedCustomRoles.Manager;

internal class SpawnManager
{
    public static readonly IReadOnlyDictionary<string, string> colorMap = new Dictionary<string, string>
    {
        { "pink", "#FF96DE" },
        { "red", "#C50000" },
        { "brown", "#944710" },
        { "silver", "#A0A0A0" },
        { "light_green", "#32CD32" },
        { "crimson", "#DC143C" },
        { "cyan", "#00B7EB" },
        { "aqua", "#00FFFF" },
        { "deep_pink", "#FF1493" },
        { "tomato", "#FF6448" },
        { "yellow", "#FAFF86" },
        { "magenta", "#FF0090" },
        { "blue_green", "#4DFFB8" },
        { "orange", "#FF9966" },
        { "lime", "#BFFF00" },
        { "green", "#228B22" },
        { "emerald", "#50C878" },
        { "carmine", "#960018" },
        { "nickel", "#727472" },
        { "mint", "#98FB98" },
        { "army_green", "#4B5320" },
        { "pumpkin", "#EE7600" }
    };

    internal static readonly HashSet<RoleTypeId> SpawnEvaluatedRoles =
    [
        RoleTypeId.ClassD,
        RoleTypeId.Scientist,
        RoleTypeId.NtfPrivate,
        RoleTypeId.NtfSergeant,
        RoleTypeId.NtfCaptain,
        RoleTypeId.NtfSpecialist,
        RoleTypeId.ChaosConscript,
        RoleTypeId.ChaosMarauder,
        RoleTypeId.ChaosRepressor,
        RoleTypeId.ChaosRifleman,
        RoleTypeId.Tutorial,
        RoleTypeId.Scp049,
        RoleTypeId.Scp0492,
        RoleTypeId.Scp079,
        RoleTypeId.Scp173,
        RoleTypeId.Scp939,
        RoleTypeId.Scp096,
        RoleTypeId.Scp106,
        RoleTypeId.Scp3114,
        RoleTypeId.FacilityGuard
    ];

    public static void ClearCustomTypes(Player player)
    {
        if (SummonedCustomRole.TryGet(player, out var role))
            role.Destroy();
    }

    public static IEnumerator<float> AsyncPlayerSpawner(Player player, int id, bool doBypassRoleOverwrite = true)
    {
        yield return Timing.WaitForSeconds(0.1f);
        SummonCustomSubclass(player, id, doBypassRoleOverwrite);
    }

    public static void SummonCustomSubclass(Player player, int id, bool doBypassRoleOverwrite = true)
    {
        try
        {
            if (!CustomRole.CustomRoles.TryGetValue(id, out var Role) || Role is null)
            {
                LogManager.Warn(
                    $"Sorry but the role with the Id {id} is not registered inside UncomplicatedCustomRoles!",
                    "CR0092");
                return;
            }

            if (Role.SpawnSettings is null)
            {
                LogManager.Warn(
                    $"Tried to spawn a custom role without spawn_settings, aborting the SummonCustomSubclass(...) action!\nRole: {Role.Name} ({Role.Id})",
                    "CR0093");
                return;
            }

            if (!doBypassRoleOverwrite && !Role.SpawnSettings.CanReplaceRoles.Contains(player.Role))
            {
                LogManager.Debug(
                    $"Can't spawn the player {player.Nickname} as UCR custom role {Role.Name} because it's role is not in the overwrittable list of custom role!\nStrange because this should be managed correctly by the plugin!");
                return;
            }

            // This will allow us to avoid the loop of another OnSpawning
            Spawn.Spawning.Add(player.PlayerId);

            var BasicPosition = player.Position;

            var SpawnFlag = RoleSpawnFlags.None;

            if (Role.SpawnSettings.Spawn == SpawnType.KeepRoleSpawn)
                SpawnFlag = RoleSpawnFlags.UseSpawnpoint;

            UcrSpawnContext.Enter();
            try
            {
                player.SetRole(Role.Role, RoleChangeReason.Respawn, SpawnFlag);
            }
            finally
            {
                UcrSpawnContext.Exit();
            }

            if (Role.SpawnSettings.Spawn == SpawnType.KeepCurrentPositionSpawn)
                player.Position = BasicPosition;

            if (SpawnFlag == RoleSpawnFlags.None)
                switch (Role.SpawnSettings.Spawn)
                {
                    case SpawnType.ZoneSpawn:
                        if (Role.SpawnSettings.SpawnZones is null || Role.SpawnSettings.SpawnZones.Count is 0)
                        {
                            LogManager.Warn(
                                $"Failed to spawn player {player.Nickname} ({player.PlayerId}) as CustomRole {Role.Name} ({Role.Id}): spawn is ZoneSpawn but spawn_zones is empty, keeping the previous position...");
                            player.Position = BasicPosition;
                            break;
                        }

                        var zone = Role.SpawnSettings.SpawnZones.RandomItem();
                        var zoneRoom = Room.List.Where(room =>
                            room.Zone == zone && room.GameObject.GetComponentInChildren<TeslaGate>() is null &&
                            room.Name is not RoomName.EzEvacShelter).RandomValue();

                        if (zoneRoom is null)
                        {
                            LogManager.Warn(
                                $"Failed to spawn player {player.Nickname} ({player.PlayerId}) as CustomRole {Role.Name} ({Role.Id}): no valid room found in zone {zone}, keeping the previous position...");
                            player.Position = BasicPosition;
                            break;
                        }

                        player.Position = zoneRoom.Position.AddY(1.5f);
                        break;
                    case SpawnType.CompleteRandomSpawn:
                        var randomRoom = Room.List
                            .Where(room => room.GameObject.GetComponentInChildren<TeslaGate>() is null).RandomValue();

                        if (randomRoom is null)
                        {
                            player.Position = BasicPosition;
                            break;
                        }

                        player.Position = randomRoom.Position.AddY(1.5f);
                        break;
                    case SpawnType.RoomsSpawn:
                        if (Role.SpawnSettings.SpawnRooms is null || Role.SpawnSettings.SpawnRooms.Count is 0)
                        {
                            LogManager.Warn(
                                $"Failed to spawn player {player.Nickname} ({player.PlayerId}) as CustomRole {Role.Name} ({Role.Id}): spawn is RoomsSpawn but spawn_rooms is empty, keeping the previous position...");
                            player.Position = BasicPosition;
                            break;
                        }

                        var roomType = Role.SpawnSettings.SpawnRooms.RandomItem();

                        var room = Room.List.Where(r =>
                            r is not null && r.GameObject.name.RemoveBracketsOnEndOfName() == roomType).RandomValue();

                        if (room is null)
                        {
                            LogManager.Error("Failed to load room with Room Name " + roomType +
                                             "!\nMake sure it exists!");
                            player.Position = BasicPosition;
                            break;
                        }

                        player.Position = room.Position.AddY(1.5f);

                        break;
                    case SpawnType.SpawnPointSpawn:
                        if (Role.SpawnSettings.SpawnPoints is not null && Role.SpawnSettings.SpawnPoints.Count > 0 &&
                            SpawnPoint.TryGet(Role.SpawnSettings.SpawnPoints.RandomItem(), out var spawn))
                        {
                            spawn.Spawn(player);
                        }
                        else
                        {
                            LogManager.Warn(
                                $"Failed to spawn player {player.Nickname} ({player.PlayerId}) as CustomRole {Role.Name} ({Role.Id}): none of the configured SpawnPoints ({(Role.SpawnSettings.SpawnPoints is null || Role.SpawnSettings.SpawnPoints.Count is 0 ? "none set" : string.Join(", ", Role.SpawnSettings.SpawnPoints))}) exists, keeping the previous position...");
                            player.Position = BasicPosition;
                        }

                        break;
                    case SpawnType.ClassDCell:
                        player.Position = RoleTypeId.ClassD.GetRandomSpawnLocation();
                        break;
                    case SpawnType.RoleSpawn:
                        if (Role.SpawnSettings.SpawnRoles is null || Role.SpawnSettings.SpawnRoles.Count is 0)
                        {
                            LogManager.Warn(
                                $"Failed to spawn player {player.Nickname} ({player.PlayerId}) as CustomRole {Role.Name} ({Role.Id}): spawn is RoleSpawn but spawn_roles is empty, keeping the previous position...");
                            player.Position = BasicPosition;
                            break;
                        }

                        var roleSpawn = Role.SpawnSettings.SpawnRoles.RandomItem().GetRandomSpawnLocation();
                        player.Position = roleSpawn != Vector3.zero ? roleSpawn : BasicPosition;
                        break;
                }

            SummonSubclassApplier(player, Role);
        }
        catch (Exception ex)
        {
            LogManager.Error(ex.ToString(), "SP0002");
        }
    }

    public static void SummonSubclassApplier(Player Player, ICustomRole Role)
    {
        try
        {
            if (Role.CustomInventoryLimits is Dictionary<ItemCategory, sbyte> inventoryLimits &&
                inventoryLimits.Count > 0)
                foreach (var category in inventoryLimits)
                    Player.SetCategoryLimit(category.Key, category.Value);

            Player.ResetInventory(Role.Inventory);

            LogManager.Silent($"Can we give any CustomItem? {Role.CustomItemsInventory.Count}");

            if (Role.CustomItemsInventory.Any())
                foreach (var itemId in Role.CustomItemsInventory)
                    if (!Player.IsInventoryFull)
                        try
                        {
                            if (UCI.HasCustomItem(itemId, out _))
                            {
                                LogManager.Debug($"Going to give UCI CustomItem {itemId} to {Player.PlayerId}");
                                UCI.GiveCustomItem(itemId, Player);
                            }
                            else
                            {
                                LogManager.Debug($"Going to give EXILED CustomItem {itemId} to {Player.PlayerId}");
                                ECI.GiveCustomItem(itemId, Player);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogManager.Error(
                                $"Failed to give the custom item {itemId} to player {Player.PlayerId} ({Player.Nickname})! Exception: {ex}");
                        }

            Player.ClearAmmo();

            if (Role.Ammo is not null && Role.Ammo.GetType() == typeof(Dictionary<ItemType, ushort>) && Role.Ammo.Any())
                foreach (var Ammo in Role.Ammo)
                {
                    if (Ammo.Value > Player.GetAmmoLimit(Ammo.Key))
                        Player.SetAmmoLimit(Ammo.Key, Ammo.Value);

                    Player.AddAmmo(Ammo.Key, Ammo.Value);
                }

            // Reset the inventory if we need to add the old one
            if (PlayerEventHandler.RespawnInventoryQueue.TryGetValue(Player.PlayerId, out var oldInventory))
            {
                Player.ClearInventory();
                Player.ClearAmmo();

                foreach (var item in oldInventory.Item1)
                    if (!oldInventory.Item3)
                    {
                        Player.AddItem(item);
                    }
                    else
                    {
                        var pickup = Pickup.Create(item, Player.Position);
                        if (pickup is null)
                            continue;
                        pickup.Spawn();
                    }

                foreach (var item in oldInventory.Item2)
                    if (!oldInventory.Item3)
                    {
                        Player.Inventory.ServerAddAmmo(item.Key, item.Value);
                    }
                    else
                    {
                        var pickup = Pickup.Create(item.Key, Player.Position);
                        if (pickup is null)
                            continue;
                        pickup.Spawn();
                    }

                PlayerEventHandler.RespawnInventoryQueue.TryRemove(Player.PlayerId, out _);
            }

            var InfoArea = Player.ReferenceHub.nicknameSync.Network_playerInfoToShow;

            // Apply every required stats
            Role.Health?.Apply(Player);
            Role.Ahp?.Apply(Player);
            Role.HumeShield?.Apply(Player);
            Role.Stamina?.Apply(Player);

            if (Role.Scale != Vector3.zero && Role.Scale != Vector3.one)
                Player.Scale = Role.Scale;

            List<IEffect> PermanentEffects = [];
            if (Role.Effects != null && Role.Effects.Any())
                foreach (IEffect effect in Role.Effects)
                {
                    if (effect.Duration < 0)
                    {
                        effect.Duration = int.MaxValue;
                        PermanentEffects.Add(effect);

                        Player.ReferenceHub.ForceApplyEffect(effect.EffectType, effect.Intensity, float.MaxValue);
                        continue;
                    }

                    LogManager.Debug(
                        $"Enabling effect {effect.EffectType} to {Player.Nickname} for {effect.Duration} (i:{effect.Intensity})");
                    Player.ReferenceHub.ForceApplyEffect(effect.EffectType, effect.Intensity, effect.Duration);
                }

            LogManager.Silent($"Found {PermanentEffects.Count} permament effects");

            if (Role.SpawnBroadcast != string.Empty)
            {
                Player.ClearBroadcasts();
                Player.SendBroadcast(Role.SpawnBroadcast, Role.SpawnBroadcastDuration);
            }

            if (Role.SpawnHint != string.Empty)
                Player.SendHint(Role.SpawnHint, Role.SpawnHintDuration);

            Triplet<string, string, bool>? Badge = null;
            if (Role.BadgeName is not null && Role.BadgeName.Length > 1 && Role.BadgeColor is not null &&
                Role.BadgeColor.Length > 2)
            {
                Badge = new Triplet<string, string, bool>(Player.ReferenceHub.serverRoles.Network_myText ?? "",
                    Player.ReferenceHub.serverRoles.Network_myColor ?? "",
                    Player.ReferenceHub.serverRoles.HasBadgeHidden);
                LogManager.Debug(
                    $"Badge detected, putting {Role.BadgeName}@{Role.BadgeColor} to player {Player.PlayerId}");

                Player.ReferenceHub.serverRoles.SetText(Role.BadgeName.Replace("@hidden", ""));
                Player.ReferenceHub.serverRoles.SetColor(Role.BadgeColor);

                if (Role.BadgeName.Contains("@hidden"))
                    if (Player.ReferenceHub.serverRoles.TryHideTag())
                        LogManager.Debug("Tag successfully hidden!");
            }

            // Changing nickname if needed
            var ChangedNick = false;
            if (Plugin.Instance.Config.AllowNicknameEdit && !string.IsNullOrEmpty(Role.Nickname))
            {
                var Nick = PlaceholderManager.ApplyPlaceholders(Role.Nickname, Player, Role);
                if (Role.Nickname.Contains(","))
                    Player.DisplayName = Nick.Split(',').RandomItem();
                else
                    Player.DisplayName = Nick;

                if (Plugin.Instance.Config.OverrideRpNames)
                    Timing.CallDelayed(3f, () => // Override RPNames shit (sowwy andrew)
                    {
                        if (Role.Nickname.Contains(","))
                            Player.DisplayName = Nick.Split(',').RandomItem();
                        else
                            Player.DisplayName = Nick;
                    });

                ChangedNick = true;
            }

            // Roll out custom info
            CustomInfo customInfo = new(Player, Role);

            LogManager.Debug($"{Player} successfully spawned as {Role.Name} ({Role.Id})!");

            SummonedCustomRole roleInstance =
                new(Player, Role, Badge, PermanentEffects, InfoArea, customInfo, ChangedNick);

            customInfo.UpdateInfo(Player);

            var escapeController = Player.GameObject.AddComponent<EscapeController>();
            escapeController.Init(roleInstance);

            if (Spawn.Spawning.Contains(Player.PlayerId))
                Spawn.Spawning.Remove(Player.PlayerId);

            if (API.Features.Escape.Bucket.Contains(Player.PlayerId))
                API.Features.Escape.Bucket.Remove(Player.PlayerId);

            LogManager.Debug($"{Player} successfully spawned as {Role.Name} ({Role.Id})! [2VDS]");
        }
        catch (Exception ex)
        {
            LogManager.Error(ex.ToString(), "SP0001");
        }
    }

    public static KeyValuePair<bool, object>? ParseEscapeRole(Dictionary<string, string> roleAfterEscape, Player player)
    {
        Dictionary<Team, KeyValuePair<bool, object>?> AsCuffedByInternalTeam = new();
        Dictionary<uint, KeyValuePair<bool, object>?> AsCuffedByCustomTeam = new();
        // cuffed by InternalTeam FoundationForces
        //   0     1       2             3           = 4
        Dictionary<int, KeyValuePair<bool, object>?> AsCuffedByCustomRole = new();
        KeyValuePair<bool, object>? Default = new(false, RoleTypeId.Spectator);

        foreach (var kvp in roleAfterEscape)
        {
            var Data = ParseEscapeString(kvp.Value);
            if (kvp.Key is "default")
            {
                Default = Data;
            }
            else
            {
                var Elements = kvp.Key.Split(' ').ToList();

                if (Elements.Count != 4)
                {
                    LogManager.Warn(
                        $"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {Elements.Count} args!\nSource: {kvp.Key}");
                    return new KeyValuePair<bool, object>(false, RoleTypeId.Spectator);
                }

                if (Elements[0] is not "cuffed")
                {
                    LogManager.Warn(
                        $"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {Elements.Count} args!\nSource: {kvp.Key}");
                    return new KeyValuePair<bool, object>(false, RoleTypeId.Spectator);
                }

                if (Elements[1] is not "by")
                {
                    LogManager.Warn(
                        $"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {Elements.Count} args!\nSource: {kvp.Key}");
                    return new KeyValuePair<bool, object>(false, RoleTypeId.Spectator);
                }

                if ((Elements[2] is "InternalTeam" || Elements[2] is "IT") && Enum.TryParse(Elements[3], out Team team))
                    AsCuffedByInternalTeam.TryAdd(team, Data);
                else if ((Elements[2] is "CustomTeam" || Elements[2] is "CT") &&
                         uint.TryParse(Elements[3], out var customTeam))
                    AsCuffedByCustomTeam.TryAdd(customTeam, Data);
                else if ((Elements[2] is "CustomRole" || Elements[2] is "CR") &&
                         int.TryParse(Elements[3], out var id) && CustomRole.CustomRoles.ContainsKey(id))
                    AsCuffedByCustomRole.TryAdd(id, Data);
                else
                    LogManager.Warn(
                        $"Function SpawnManager::ParseEscapeRole[2](<...>) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalTeam / IT / CustomRole / CR. Found: {Elements[2]}\n- The target is not a CustomRole / InternalRole. Found: {Elements[3]}");
            }
        }

        // Now let's assign
        if (!player.IsDisarmed)
            return Default;
        if (player.IsDisarmed && player.DisarmedBy is not null)
            if (player.DisarmedBy.TryGetSummonedInstance(out var role) &&
                AsCuffedByCustomRole.TryGetValue(role.Role.Id, out var crEscapeRole))
                return crEscapeRole;
            else if (UCT.TryGetCustomTeamId(player.DisarmedBy, out var uctTeamId) &&
                     AsCuffedByCustomTeam.TryGetValue(uctTeamId, out var uctEscapeRole))
                return uctEscapeRole;
            else if (AsCuffedByInternalTeam.TryGetValue(player.DisarmedBy.Team, out var internalEscapeRole))
                return internalEscapeRole;

        LogManager.Silent(
            $"Returing default type for escaping evaluation of player {player.PlayerId} who's cuffed by {player.DisarmedBy?.Team}");
        return Default;
    }

    public static KeyValuePair<bool, object>? ParseEscapeString(string escape)
    {
        if (escape is "Deny" or "deny" or "DENY")
            return null;

        var Elements = escape.Split(' ').ToList();
        if (Elements.Count != 2)
        {
            LogManager.Warn(
                $"Failed to parse an EscapeString[value]: syntax should be <source> <id> (2 args), found {Elements.Count} args!\nSource: {escape}");
            return new KeyValuePair<bool, object>(false, RoleTypeId.Spectator);
        }

        if ((Elements[0] is "CustomRole" || Elements[0] is "CR") && int.TryParse(Elements[1], out var customRoleId))
            return new KeyValuePair<bool, object>(true, customRoleId);
        if ((Elements[0] is "InternalRole" || Elements[0] is "IR") && Enum.TryParse(Elements[1], out RoleTypeId role))
            return new KeyValuePair<bool, object>(false, role);
        LogManager.Warn(
            $"Function SpawnManager::ParseEscapeString(string escape) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalRole / IR / CustomRole / CR. Found: {Elements[0]}\n- The target is not a CustomRole / InternalRole. Found: {Elements[1]}");

        return new KeyValuePair<bool, object>(false, RoleTypeId.Spectator);
    }

#nullable enable
    public static ICustomRole? DoEvaluateSpawnForPlayer(Player player, RoleTypeId? role = null)
    {
        role ??= player.Role;

        if (role is null)
            return null;

        var NewRole = (RoleTypeId)role;

        if (player.HasCustomRole())
        {
            LogManager.Debug("Was evalutating role select for an already custom role player, stopping");
            return null;
        }

        Dictionary<RoleTypeId, List<ICustomRole>> RolePercentage = new();
        foreach (var evaluated in SpawnEvaluatedRoles)
            RolePercentage[evaluated] = [];

        foreach (var Role in CustomRole.CustomRoles.Values.Where(cr => cr.SpawnSettings is not null))
            if (!Role.IgnoreSpawnSystem && Player.ReadyList.Count() >= Role.SpawnSettings?.MinPlayers &&
                SummonedCustomRole.Count(Role) < Role.SpawnSettings.MaxPlayers)
            {
                if (Role.SpawnSettings.RequiredPermission is not null)
                {
                    static bool CheckPermission(Player player, string permission)
                    {
                        if (Enum.TryParse(permission, out PlayerPermissions playerPermissions))
                            return player.HasPermission(playerPermissions);

                        return player.HasAnyPermission(permission);
                    }

                    static IEnumerable<string> ExtractPermissions(object obj)
                    {
                        switch (obj)
                        {
                            case string s when !string.IsNullOrWhiteSpace(s):
                                return [s];
                            case IEnumerable enumerable:
                            {
                                var list = new List<string>();
                                foreach (var item in enumerable)
                                {
                                    if (item is null) continue;
                                    var s = item.ToString();
                                    if (!string.IsNullOrWhiteSpace(s)) list.Add(s);
                                }

                                return list;
                            }
                            default:
                                return [];
                        }
                    }

                    var permsList = ExtractPermissions(Role.SpawnSettings.RequiredPermission).ToList();
                    if (permsList.Any())
                    {
                        var hasAll = permsList.All(p => CheckPermission(player, p));
                        if (!hasAll)
                        {
                            LogManager.Debug(
                                $"Player {player.PlayerId} doesn't have the required permission(s) to spawn as role {Role.Name} ({Role.Id}), skipping... Player Permissions: {string.Join(", ", player.GetPermissions())}, Required permission(s): {string.Join(", ", permsList)}");
                            continue;
                        }
                    }
                }

                foreach (var RoleType in Role.SpawnSettings.CanReplaceRoles)
                {
                    if (!RolePercentage.TryGetValue(RoleType, out var bucket))
                        continue;

                    for (var a = 0; a < Role.SpawnSettings.SpawnChance; a++)
                        bucket.Add(Role);
                }
            }

        if (RolePercentage.ContainsKey(NewRole))
            if (Random.Range(0, 100) < RolePercentage[NewRole].Count)
                return CustomRole.CustomRoles[RolePercentage[NewRole].RandomItem().Id];

        return null;
    }

    public static void AnnounceScpTermination(ReferenceHub scp, DamageHandlerBase hit)
    {
        var announcement1 = hit.CassieDeathAnnouncement.Announcement;
        var subtitleParts1 = hit.CassieDeathAnnouncement.SubtitleParts;
        if (string.IsNullOrEmpty(announcement1))
            return;
        foreach (var cassieAnnouncement in CassieAnnouncementDispatcher.AllAnnouncementsPreview)
            if (cassieAnnouncement is CassieScpTerminationAnnouncement terminationAnnouncement &&
                terminationAnnouncement._announcementTts == announcement1 &&
                SubtitlePart.CheckEqualValues(terminationAnnouncement._subtitles, subtitleParts1))
            {
                terminationAnnouncement._victims.Add(new Footprint(scp));
                terminationAnnouncement._remainingWait = 1f;
                return;
            }

        var ev = new CassieQueuingScpTerminationEventArgs(scp, announcement1, subtitleParts1, hit);
        ServerEvents.OnCassieQueuingScpTermination(ev);
        if (!ev.IsAllowed)
            return;
        var announcement2 = ev.Announcement;
        var subtitleParts2 = ev.SubtitleParts;
        new CassieScpTerminationAnnouncement(new Footprint(scp), announcement2, subtitleParts2).AddToQueue();
        ServerEvents.OnCassieQueuedScpTermination(
            new CassieQueuedScpTerminationEventArgs(scp, announcement2, subtitleParts2, hit));
    }

    internal static IEnumerable<Player> LoadAppearanceAffectedPlayers(Player target)
    {
        List<Player> result = [];
        foreach (var player in Player.ReadyList.Where(p => p.PlayerId != target.PlayerId))
            if (!player.TryGetSummonedInstance(out var role) || !role.HasModule<NotAffectedByAppearance>())
                result.Add(player);

        return result;
    }
}