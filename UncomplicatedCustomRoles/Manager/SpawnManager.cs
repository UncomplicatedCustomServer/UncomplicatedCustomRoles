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
using UncomplicatedCustomRoles.API.Events;
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
    public static readonly IReadOnlyDictionary<string, string> ColorMap = new Dictionary<string, string>
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
        if (SummonedCustomRole.TryGet(player, out SummonedCustomRole role))
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
            if (!CustomRole.CustomRoles.TryGetValue(id, out ICustomRole Role) || Role is null)
            {
                LogManager.Warn($"Sorry but the role with the Id {id} is not registered inside UncomplicatedCustomRoles!", "CR0092");
                return;
            }

            if (Role.SpawnSettings is null)
            {
                LogManager.Warn($"Tried to spawn a custom role without spawn_settings, aborting the SummonCustomSubclass(...) action!\nRole: {Role.Name} ({Role.Id})", "CR0093");
                return;
            }

            if (!doBypassRoleOverwrite && !Role.SpawnSettings.CanReplaceRoles.Contains(player.Role))
            {
                LogManager.Debug($"Can't spawn the player {player.Nickname} as UCR custom role {Role.Name} because it's role is not in the overwrittable list of custom role!\nStrange because this should be managed correctly by the plugin!");
                return;
            }

            CustomRoleSpawningEventArgs spawningArgs = new(player, Role);
            CustomRoleEvents.OnSpawning(spawningArgs);
            if (!spawningArgs.IsAllowed)
            {
                LogManager.Debug($"Spawn of player {player.Nickname} as CustomRole {Role.Name} ({Role.Id}) denied by an external plugin through CustomRoleEvents.Spawning");
                return;
            }

            // This will allow us to avoid the loop of another OnSpawning
            Spawn.Spawning.Add(player.PlayerId);

            Vector3 BasicPosition = player.Position;

            RoleSpawnFlags SpawnFlag = RoleSpawnFlags.None;

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
                            LogManager.Warn($"Failed to spawn player {player.Nickname} ({player.PlayerId}) as CustomRole {Role.Name} ({Role.Id}): spawn is ZoneSpawn but spawn_zones is empty, keeping the previous position...");
                            player.Position = BasicPosition;
                            break;
                        }

                        FacilityZone zone = Role.SpawnSettings.SpawnZones.RandomItem();
                        Room zoneRoom = Room.List.Where(room => room.Zone == zone && room.GameObject.GetComponentInChildren<TeslaGate>() is null && room.Name is not RoomName.EzEvacShelter).RandomValue();

                        if (zoneRoom is null)
                        {
                            LogManager.Warn($"Failed to spawn player {player.Nickname} ({player.PlayerId}) as CustomRole {Role.Name} ({Role.Id}): no valid room found in zone {zone}, keeping the previous position...");
                            player.Position = BasicPosition;
                            break;
                        }

                        player.Position = zoneRoom.Position.AddY(1.5f);
                        break;
                    case SpawnType.CompleteRandomSpawn:
                        Room randomRoom = Room.List.Where(room => room.GameObject.GetComponentInChildren<TeslaGate>() is null).RandomValue();

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
                            LogManager.Warn($"Failed to spawn player {player.Nickname} ({player.PlayerId}) as CustomRole {Role.Name} ({Role.Id}): spawn is RoomsSpawn but spawn_rooms is empty, keeping the previous position...");
                            player.Position = BasicPosition;
                            break;
                        }

                        string roomType = Role.SpawnSettings.SpawnRooms.RandomItem();

                        Room room = Room.List.Where(r => r is not null && r.GameObject.name.RemoveBracketsOnEndOfName() == roomType).RandomValue();

                        if (room is null)
                        {
                            LogManager.Error("Failed to load room with Room Name " + roomType + "!\nMake sure it exists!");
                            player.Position = BasicPosition;
                            break;
                        }

                        player.Position = room.Position.AddY(1.5f);

                        break;
                    case SpawnType.SpawnPointSpawn:
                        if (Role.SpawnSettings.SpawnPoints is not null && Role.SpawnSettings.SpawnPoints.Count > 0 && SpawnPoint.TryGet(Role.SpawnSettings.SpawnPoints.RandomItem(), out SpawnPoint spawn))
                        {
                            spawn.Spawn(player);
                        }
                        else
                        {
                            LogManager.Warn($"Failed to spawn player {player.Nickname} ({player.PlayerId}) as CustomRole {Role.Name} ({Role.Id}): none of the configured SpawnPoints ({(Role.SpawnSettings.SpawnPoints is null || Role.SpawnSettings.SpawnPoints.Count is 0 ? "none set" : string.Join(", ", Role.SpawnSettings.SpawnPoints))}) exists, keeping the previous position...");
                            player.Position = BasicPosition;
                        }

                        break;
                    case SpawnType.ClassDCell:
                        player.Position = RoleTypeId.ClassD.GetRandomSpawnLocation();
                        break;
                    case SpawnType.RoleSpawn:
                        if (Role.SpawnSettings.SpawnRoles is null || Role.SpawnSettings.SpawnRoles.Count is 0)
                        {
                            LogManager.Warn($"Failed to spawn player {player.Nickname} ({player.PlayerId}) as CustomRole {Role.Name} ({Role.Id}): spawn is RoleSpawn but spawn_roles is empty, keeping the previous position...");
                            player.Position = BasicPosition;
                            break;
                        }

                        Vector3 roleSpawn = Role.SpawnSettings.SpawnRoles.RandomItem().GetRandomSpawnLocation();
                        player.Position = roleSpawn != Vector3.zero ? roleSpawn : BasicPosition;
                        break;
                }

            SummonSubclassApplier(player, Role, true);
        }
        catch (Exception ex)
        {
            LogManager.Error(ex.ToString(), "SP0002");
        }
        finally
        {
            Spawn.Spawning.Remove(player.PlayerId);
        }
    }

    public static void SummonSubclassApplier(Player Player, ICustomRole Role)
    {
        SummonSubclassApplier(Player, Role, false);
    }

    internal static void SummonSubclassApplier(Player Player, ICustomRole Role, bool spawningEventAlreadyFired)
    {
        try
        {
            if (!spawningEventAlreadyFired)
            {
                CustomRoleSpawningEventArgs spawningArgs = new(Player, Role);
                CustomRoleEvents.OnSpawning(spawningArgs);
                if (!spawningArgs.IsAllowed)
                {
                    LogManager.Debug($"Spawn of player {Player.Nickname} as CustomRole {Role.Name} ({Role.Id}) denied by an external plugin through CustomRoleEvents.Spawning");
                    return;
                }
            }

            if (Role.CustomInventoryLimits is { Count: > 0 } inventoryLimits)
                foreach (KeyValuePair<ItemCategory, sbyte> category in inventoryLimits)
                    Player.SetCategoryLimit(category.Key, category.Value);

            Player.ResetInventory(Role.Inventory);

            if (Role.CustomItemsInventory is { Count: > 0 })
                foreach (uint itemId in Role.CustomItemsInventory)
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
                            LogManager.Error($"Failed to give the custom item {itemId} to player {Player.PlayerId} ({Player.Nickname})! Exception: {ex}");
                        }

            Player.ClearAmmo();

            if (Role.Ammo is { Count: > 0 })
                foreach (KeyValuePair<ItemType, ushort> Ammo in Role.Ammo)
                {
                    if (Ammo.Value > Player.GetAmmoLimit(Ammo.Key))
                        Player.SetAmmoLimit(Ammo.Key, Ammo.Value);

                    Player.AddAmmo(Ammo.Key, Ammo.Value);
                }

            if (PlayerEventHandler.RespawnInventoryQueue.TryRemove(Player.PlayerId, out Tuple<List<ItemType>, Dictionary<ItemType, ushort>, bool> oldInventory))
            {
                if (!oldInventory.Item3)
                {
                    Player.ClearInventory();
                    Player.ClearAmmo();

                    foreach (ItemType item in oldInventory.Item1)
                        Player.AddItem(item);

                    foreach (KeyValuePair<ItemType, ushort> ammo in oldInventory.Item2)
                        Player.Inventory.ServerAddAmmo(ammo.Key, ammo.Value);
                }
                else
                {
                    foreach (ItemType item in oldInventory.Item1)
                        Pickup.Create(item, Player.Position)?.Spawn();

                    foreach (KeyValuePair<ItemType, ushort> ammo in oldInventory.Item2)
                        Pickup.Create(ammo.Key, Player.Position)?.Spawn();
                }
            }

            PlayerInfoArea InfoArea = Player.ReferenceHub.nicknameSync.Network_playerInfoToShow;

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

                    LogManager.Debug($"Enabling effect {effect.EffectType} to {Player.Nickname} for {effect.Duration} (i:{effect.Intensity})");
                    Player.ReferenceHub.ForceApplyEffect(effect.EffectType, effect.Intensity, effect.Duration);
                }

            LogManager.Silent($"Found {PermanentEffects.Count} permament effects");

            if (!string.IsNullOrEmpty(Role.SpawnBroadcast))
            {
                Player.ClearBroadcasts();
                Player.SendBroadcast(Role.SpawnBroadcast, Role.SpawnBroadcastDuration);
            }

            if (!string.IsNullOrEmpty(Role.SpawnHint))
                Player.SendHint(Role.SpawnHint, Role.SpawnHintDuration);

            Triplet<string, string, bool>? Badge = null;
            if (Role.BadgeName is not null && Role.BadgeName.Length > 1 && Role.BadgeColor is not null && Role.BadgeColor.Length > 2)
            {
                Badge = new Triplet<string, string, bool>(Player.ReferenceHub.serverRoles.Network_myText ?? "", Player.ReferenceHub.serverRoles.Network_myColor ?? "", Player.ReferenceHub.serverRoles.HasBadgeHidden);
                LogManager.Debug($"Badge detected, putting {Role.BadgeName}@{Role.BadgeColor} to player {Player.PlayerId}");

                Player.ReferenceHub.serverRoles.SetText(Role.BadgeName.Replace("@hidden", ""));
                Player.ReferenceHub.serverRoles.SetColor(Role.BadgeColor);

                if (Role.BadgeName.Contains("@hidden"))
                    if (Player.ReferenceHub.serverRoles.TryHideTag())
                        LogManager.Debug("Tag successfully hidden!");
            }

            // Changing nickname if needed
            bool ChangedNick = false;
            string appliedNick = null;
            if (Plugin.Instance.Config.AllowNicknameEdit && !string.IsNullOrEmpty(Role.Nickname))
            {
                string Nick = PlaceholderManager.ApplyPlaceholders(Role.Nickname, Player, Role);

                appliedNick = Role.Nickname.Contains(",") ? Nick.Split(',').RandomItem().Trim() : Nick;
                Player.DisplayName = appliedNick;

                ChangedNick = true;
            }

            // Roll out custom info
            CustomInfo customInfo = new(Player, Role);

            LogManager.Debug($"{Player} successfully spawned as {Role.Name} ({Role.Id})!");

            SummonedCustomRole roleInstance = new(Player, Role, Badge, PermanentEffects, InfoArea, customInfo, ChangedNick);

            roleInstance.AppliedNickname = appliedNick;

            if (appliedNick is not null)
                customInfo.Nickname = appliedNick;

            customInfo.UpdateInfo(Player);

            if (appliedNick is not null && Plugin.Instance.Config.OverrideRpNames)
                roleInstance.NicknameReapplyCoroutine = Timing.CallDelayed(3f, () =>
                {
                    if (roleInstance.IsValid && SummonedCustomRole.Get(roleInstance.Player) == roleInstance)
                        roleInstance.Player.DisplayName = appliedNick;
                });

            EscapeController escapeController = Player.GameObject.AddComponent<EscapeController>();
            escapeController.Init(roleInstance);

            if (Spawn.Spawning.Contains(Player.PlayerId))
                Spawn.Spawning.Remove(Player.PlayerId);

            if (API.Features.Escape.Bucket.Contains(Player.PlayerId))
                API.Features.Escape.Bucket.Remove(Player.PlayerId);

            CustomRoleEvents.OnSpawned(new CustomRoleSpawnedEventArgs(roleInstance));

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

        foreach (KeyValuePair<string, string> kvp in roleAfterEscape)
        {
            KeyValuePair<bool, object>? Data = ParseEscapeString(kvp.Value);
            if (kvp.Key is "default")
            {
                Default = Data;
            }
            else
            {
                string[] Elements = kvp.Key.Split(' ');

                if (Elements.Length != 4 || Elements[0] is not "cuffed" || Elements[1] is not "by")
                {
                    LogManager.Warn($"Failed to parse an EscapeRole[key]: syntax should be 'cuffed by <source> <id>' (4 args), found {Elements.Length} args!\nSource: {kvp.Key}");
                    continue;
                }

                if ((Elements[2] is "InternalTeam" || Elements[2] is "IT") && Enum.TryParse(Elements[3], out Team team))
                    AsCuffedByInternalTeam.TryAdd(team, Data);
                else if ((Elements[2] is "CustomTeam" || Elements[2] is "CT") && uint.TryParse(Elements[3], out uint customTeam))
                    AsCuffedByCustomTeam.TryAdd(customTeam, Data);
                else if ((Elements[2] is "CustomRole" || Elements[2] is "CR") && int.TryParse(Elements[3], out int id) && CustomRole.CustomRoles.ContainsKey(id))
                    AsCuffedByCustomRole.TryAdd(id, Data);
                else
                    LogManager.Warn($"Function SpawnManager::ParseEscapeRole[2](<...>) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalTeam / IT / CustomRole / CR. Found: {Elements[2]}\n- The target is not a CustomRole / InternalRole. Found: {Elements[3]}");
            }
        }

        // Now let's assign
        if (!player.IsDisarmed)
            return Default;
        if (player.IsDisarmed && player.DisarmedBy is not null)
        {
            if (player.DisarmedBy.TryGetSummonedInstance(out SummonedCustomRole role) && AsCuffedByCustomRole.TryGetValue(role.Role.Id, out KeyValuePair<bool, object>? crEscapeRole))
                return crEscapeRole;
            if (UCT.TryGetCustomTeamId(player.DisarmedBy, out uint uctTeamId) && AsCuffedByCustomTeam.TryGetValue(uctTeamId, out KeyValuePair<bool, object>? uctEscapeRole))
                return uctEscapeRole;
            if (AsCuffedByInternalTeam.TryGetValue(player.DisarmedBy.Team, out KeyValuePair<bool, object>? internalEscapeRole))
                return internalEscapeRole;
        }

        LogManager.Silent($"Returing default type for escaping evaluation of player {player.PlayerId} who's cuffed by {player.DisarmedBy?.Team}");
        return Default;
    }

    public static KeyValuePair<bool, object>? ParseEscapeString(string escape)
    {
        if (escape is "Deny" or "deny" or "DENY")
            return null;

        List<string> Elements = escape.Split(' ').ToList();
        if (Elements.Count != 2)
        {
            LogManager.Warn($"Failed to parse an EscapeString[value]: syntax should be <source> <id> (2 args), found {Elements.Count} args!\nSource: {escape}");
            return new KeyValuePair<bool, object>(false, RoleTypeId.Spectator);
        }

        if ((Elements[0] is "CustomRole" || Elements[0] is "CR") && int.TryParse(Elements[1], out int customRoleId))
            return new KeyValuePair<bool, object>(true, customRoleId);
        if ((Elements[0] is "InternalRole" || Elements[0] is "IR") && Enum.TryParse(Elements[1], out RoleTypeId role))
            return new KeyValuePair<bool, object>(false, role);
        LogManager.Warn($"Function SpawnManager::ParseEscapeString(string escape) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalRole / IR / CustomRole / CR. Found: {Elements[0]}\n- The target is not a CustomRole / InternalRole. Found: {Elements[1]}");

        return new KeyValuePair<bool, object>(false, RoleTypeId.Spectator);
    }

#nullable enable
    public static ICustomRole? DoEvaluateSpawnForPlayer(Player player, RoleTypeId? role = null)
    {
        role ??= player.Role;

        if (role is null)
            return null;

        RoleTypeId NewRole = (RoleTypeId)role;

        if (player.HasCustomRole())
        {
            LogManager.Debug("Was evalutating role select for an already custom role player, stopping");
            return null;
        }

        if (!SpawnEvaluatedRoles.Contains(NewRole))
            return null;

        int readyPlayers = Player.ReadyList.Count();
        List<ICustomRole> candidates = [];

        foreach (ICustomRole Role in CustomRole.CustomRoles.Values)
            if (Role.SpawnSettings is not null && !Role.IgnoreSpawnSystem && Role.SpawnSettings.SpawnDelay <= 0 && Role.SpawnSettings.CanReplaceRoles is { } canReplaceRoles && canReplaceRoles.Contains(NewRole) && readyPlayers >= Role.SpawnSettings.MinPlayers && SummonedCustomRole.Count(Role) < Role.SpawnSettings.MaxPlayers)
            {
                if (!HasRequiredPermission(player, Role))
                    continue;

                for (int a = 0; a < Role.SpawnSettings.SpawnChance; a++)
                    candidates.Add(Role);
            }

        if (candidates.Count > 0 && Random.Range(0, 100) < candidates.Count)
            return candidates.RandomItem();

        return null;
    }

    internal static bool HasRequiredPermission(Player player, ICustomRole role)
    {
        if (role.SpawnSettings?.RequiredPermission is null)
            return true;

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
                    List<string> list = new();
                    foreach (object? item in enumerable)
                    {
                        if (item is null) continue;
                        string s = item.ToString();
                        if (!string.IsNullOrWhiteSpace(s)) list.Add(s);
                    }

                    return list;
                }
                default:
                    return [];
            }
        }

        List<string> permsList = ExtractPermissions(role.SpawnSettings.RequiredPermission).ToList();
        if (!permsList.Any())
            return true;

        if (permsList.All(p => CheckPermission(player, p)))
            return true;

        LogManager.Debug($"Player {player.PlayerId} doesn't have the required permission(s) to spawn as role {role.Name} ({role.Id}), skipping... Player Permissions: {string.Join(", ", player.GetPermissions())}, Required permission(s): {string.Join(", ", permsList)}");
        return false;
    }

    public static void AnnounceScpTermination(ReferenceHub scp, DamageHandlerBase hit)
    {
        string announcement1 = hit.CassieDeathAnnouncement.Announcement;
        SubtitlePart[] subtitleParts1 = hit.CassieDeathAnnouncement.SubtitleParts;
        if (string.IsNullOrEmpty(announcement1))
            return;
        foreach (CassieAnnouncement? cassieAnnouncement in CassieAnnouncementDispatcher.AllAnnouncementsPreview)
            if (cassieAnnouncement is CassieScpTerminationAnnouncement terminationAnnouncement && terminationAnnouncement._announcementTts == announcement1 && SubtitlePart.CheckEqualValues(terminationAnnouncement._subtitles, subtitleParts1))
            {
                terminationAnnouncement._victims.Add(new Footprint(scp));
                terminationAnnouncement._remainingWait = 1f;
                return;
            }

        CassieQueuingScpTerminationEventArgs ev = new(scp, announcement1, subtitleParts1, hit);
        ServerEvents.OnCassieQueuingScpTermination(ev);
        if (!ev.IsAllowed)
            return;
        string announcement2 = ev.Announcement;
        SubtitlePart[] subtitleParts2 = ev.SubtitleParts;
        new CassieScpTerminationAnnouncement(new Footprint(scp), announcement2, subtitleParts2).AddToQueue();
        ServerEvents.OnCassieQueuedScpTermination(new CassieQueuedScpTerminationEventArgs(scp, announcement2, subtitleParts2, hit));
    }

    internal static IEnumerable<Player> LoadAppearanceAffectedPlayers(Player target)
    {
        List<Player> result = [];
        foreach (Player? player in Player.ReadyList.Where(p => p.PlayerId != target.PlayerId))
            if (!player.TryGetSummonedInstance(out SummonedCustomRole? role) || !role.HasModule<NotAffectedByAppearance>())
                result.Add(player);

        return result;
    }
}