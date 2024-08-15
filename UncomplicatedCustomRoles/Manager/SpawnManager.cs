using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Interfaces;
using UnityEngine;
using System;
using UncomplicatedCustomRoles.Extensions;
using MEC;
using PluginAPI.Core;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Struct;
using MapGeneration;
using UncomplicatedCustomRoles.API.Helpers.Imports.EXILED.Extensions;
using InventorySystem.Disarming;

// Mormora, la gente mormora
// falla tacere praticando l'allegria

namespace UncomplicatedCustomRoles.Manager
{
    internal class SpawnManager
    {
        public static void ClearCustomTypes(Player player)
        {
            if (SummonedCustomRole.TryGet(player, out SummonedCustomRole role))
                role.Destroy();
        }

        public static void SummonCustomSubclass(Player player, int id, bool doBypassRoleOverwrite = true)
        {
            // Does the role exists?
            if (!CustomRole.CustomRoles.ContainsKey(id))
            {
                LogManager.Warn($"Sorry but the role with the Id {id} is not registered inside UncomplicatedCustomRoles!", "CR0092");
                return;
            }

            ICustomRole Role = CustomRole.CustomRoles[id];

            if (Role is null)
            {
                LogManager.Warn($"Tried to spawn a custom role with the Id {id} but it seems to not exists!");
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

            // This will allow us to avoid the loop of another OnSpawning
            Spawn.Spawning.TryAdd(player.PlayerId);

            Vector3 BasicPosition = player.Position;

            RoleSpawnFlags SpawnFlag = RoleSpawnFlags.None;

            if (Role.SpawnSettings.Spawn == SpawnLocationType.KeepRoleSpawn)
                SpawnFlag = RoleSpawnFlags.UseSpawnpoint;

            player.ReferenceHub.roleManager.ServerSetRole(Role.Role, RoleChangeReason.Respawn, SpawnFlag);

            if (Role.SpawnSettings.Spawn == SpawnLocationType.KeepCurrentPositionSpawn)
                player.Position = BasicPosition;

            if (SpawnFlag == RoleSpawnFlags.None)
            {
                switch (Role.SpawnSettings.Spawn)
                {
                    case SpawnLocationType.ZoneSpawn:
                        player.Position = Map.Rooms.Where(room => room.Zone == Role.SpawnSettings.SpawnZones.RandomItem() && Map.TeslaGates.Where(tesla => tesla.Room != room).Count() < 1).RandomValue().ApiRoom.Position.AddY(0.75f);
                        break;
                    case SpawnLocationType.CompleteRandomSpawn:
                        player.Position = Map.Rooms.Where(room => Map.TeslaGates.Where(tesla => tesla.Room != room).Count() < 1).RandomValue().ApiRoom.Position.AddY(0.75f);
                        break;
                    case SpawnLocationType.RoomsSpawn:
                        LogManager.Silent($"Going to spawn CR {Role.Name} ({Role.Id}) ({player.Nickname}) at a Room - Count: {Role.SpawnSettings.SpawnRooms.Count}");
                        RoomName Room = Role.SpawnSettings.SpawnRooms.RandomItem();
                        player.Position = Map.Rooms.Where(room => room.Name == Room).FirstOrDefault()?.ApiRoom.Position.AddY(1.5f) ?? BasicPosition;
                        break;
                    case SpawnLocationType.SpawnPointSpawn:
                        if (Role.SpawnSettings.SpawnPoint is not null && SpawnPoint.TryGet(Role.SpawnSettings.SpawnPoint, out SpawnPoint spawn))
                            spawn.Spawn(player);
                        else
                        {
                            LogManager.Warn($"Failed to spawn player {player.Nickname} ({player.PlayerId}) as CustomRole {Role.Name} ({Role.Id}): selected SpawnPoint '{Role.SpawnSettings.SpawnPoint}' does not exists, set the spawn position to the previous one...");
                            player.Position = BasicPosition;
                        }
                        break;
                };
            }

            SummonSubclassApplier(player, Role);
        }

        public static void SummonSubclassApplier(Player Player, ICustomRole Role)
        {
            Timing.CallDelayed(0.75f, () =>
            {
                Player.ClearInventory();
                foreach (ItemType Item in Role.Inventory)
                    Player.AddItem(Item);

                // ClearAmmo
                Player.ReferenceHub.inventory.UserInventory.ReserveAmmo.Clear();
                Player.ReferenceHub.inventory.SendAmmoNextFrame = true;

                if (Role.Ammo.GetType() == typeof(Dictionary<ItemType, ushort>) && Role.Ammo.Count() > 0)
                    foreach (KeyValuePair<ItemType, ushort> Ammo in Role.Ammo)
                        Player.AddAmmo(Ammo.Key, Ammo.Value);

                if (Role.CustomInfo != null && Role.CustomInfo != string.Empty)
                    if (Role.OverrideRoleName)
                        Player.ApplyCustomInfoAndRoleName(Role.CustomInfo, Role.Name);
                    else
                        Player.ApplyClearCustomInfo(Role.CustomInfo);

                // Apply every required stats
                Role.Health?.Apply(Player);
                Role.Ahp?.Apply(Player);
                Role.Stamina?.Apply(Player);

                if (Role.Scale != Vector3.zero && Role.Scale != Vector3.one)
                    Player.ApplyScale(Role.Scale);


                if (Role.RoleAppearance != Role.Role)
                {
                    LogManager.Debug($"Changing the appearance of the role {Role.Id} [{Role.Name}] to {Role.RoleAppearance}");
                    Timing.CallDelayed(1f, () =>
                    {
                        Player.ChangeAppearance(Role.RoleAppearance, true);
                    });
                }
            });

            LogManager.Silent("Adding permanent effects");
            List<IEffect> PermanentEffects = new();
            if (Role.Effects.Count() > 0 && Role.Effects != null)
            {
                foreach (IEffect effect in Role.Effects)
                {
                    if (effect.Duration < 0)
                    {
                        effect.Duration = int.MaxValue;
                        PermanentEffects.Add(effect);
                        continue;
                    }
                    Player.ReferenceHub.playerEffectsController.ChangeState(effect.EffectName, effect.Intensity, effect.Duration);
                }
            }

            if (Role.SpawnBroadcast != string.Empty)
            {
                Player.ClearBroadcasts();
                Player.SendBroadcast(Role.SpawnBroadcast, Role.SpawnBroadcastDuration);
            }

            LogManager.Silent("Assigining SpawnHint");
            if (Role.SpawnHint != string.Empty)
                Player.ReceiveHint(Role.SpawnHint, Role.SpawnHintDuration);

            Triplet<string, string, bool>? Badge = null;
            if (Role.BadgeName is not null && Role.BadgeName.Length > 1 && Role.BadgeColor is not null && Role.BadgeColor.Length > 2)
            {
                Badge = new(Player.ReferenceHub.serverRoles.Network_myText ?? "", Player.ReferenceHub.serverRoles.Network_myColor ?? "", Player.ReferenceHub.serverRoles.HasBadgeHidden);
                LogManager.Debug($"Badge detected, putting {Role.BadgeName}@{Role.BadgeColor} to player {Player.PlayerId}");

                Player.ReferenceHub.serverRoles.SetText(Role.BadgeName.Replace("@hidden", ""));
                Player.ReferenceHub.serverRoles.SetColor(Role.BadgeColor);

                if (Role.BadgeName.Contains("@hidden"))
                    if (Player.ReferenceHub.serverRoles.TryHideTag())
                        LogManager.Debug("Tag successfully hidden!");
            }

            if (Role.CustomInfo != null && Role.CustomInfo != string.Empty)
                if (Role.OverrideRoleName)
                    Player.ApplyCustomInfoAndRoleName(Role.CustomInfo, Role.Name);
                else
                    Player.ApplyClearCustomInfo(Role.CustomInfo);

            // Changing nickname if needed
            bool ChangedNick = false;
            if (Plugin.Instance.Config.AllowNicknameEdit && Role.Nickname is not null && Role.Nickname != string.Empty)
            {
                string Nick = Role.Nickname.Replace("%dnumber%", new System.Random().Next(1000, 9999).ToString()).Replace("%nick%", Player.Nickname).Replace("%rand%", new System.Random().Next(0, 9).ToString()).Replace("%unitid%", Player.UnitId.ToString());
                if (Role.Nickname.Contains(","))
                    Player.DisplayNickname = Nick.Split(',').RandomItem();
                else
                    Player.DisplayNickname = Nick;

                ChangedNick = true;
            }

            LogManager.Debug($"{Player} successfully spawned as {Role.Name} ({Role.Id})!");

            //new SummonedCustomRole(Player, Role, Badge, PermanentEffects, ChangedNick);
            new SummonedCustomRole(Player, Role, Badge, PermanentEffects, ChangedNick);

            if (Spawn.Spawning.Contains(Player.PlayerId))
                Spawn.Spawning.Remove(Player.PlayerId);

            LogManager.Debug($"{Player} successfully spawned as {Role.Name} ({Role.Id})! [2VDS]");
        }

        public static KeyValuePair<bool, object> ParseEscapeRole(Dictionary<string, string> roleAfterEscape, Player player)
        {
            Dictionary<Team, KeyValuePair<bool, object>> AsCuffedByInternalTeam = new();
            // Dictionary<uint, KeyValuePair<bool, object>> AsCuffedByCustomTeam = new(); we will add the support to UCT and UIU-RS
            Dictionary<int, KeyValuePair<bool, object>> AsCuffedByCustomRole = new();
            KeyValuePair<bool, object> Default = new(false, RoleTypeId.Spectator);

            foreach (KeyValuePair<string, string> kvp in roleAfterEscape)
            {
                KeyValuePair<bool, object> Data = ParseEscapeString(kvp.Value);
                if (kvp.Key is "default")
                    Default = Data;
                else
                {
                    List<string> Elements = kvp.Key.Split(' ').ToList();

                    if (Elements.Count != 4)
                    {
                        LogManager.Warn($"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {Elements.Count} args!\nSource: {kvp.Key}");
                        return new(false, RoleTypeId.Spectator);
                    }

                    if (Elements[0] is not "cuffed")
                    {
                        LogManager.Warn($"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {Elements.Count} args!\nSource: {kvp.Key}");
                        return new(false, RoleTypeId.Spectator);
                    }

                    if (Elements[1] is not "by")
                    {
                        LogManager.Warn($"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {Elements.Count} args!\nSource: {kvp.Key}");
                        return new(false, RoleTypeId.Spectator);
                    }

                    if ((Elements[2] is "InternalTeam" || Elements[2] is "IT") && Enum.TryParse(Elements[3], out Team team))
                        AsCuffedByInternalTeam.TryAdd(team, Data);
                    else if ((Elements[2] is "CustomRole" || Elements[2] is "CR") && int.TryParse(Elements[3], out int id) && CustomRole.CustomRoles.ContainsKey(id))
                        AsCuffedByCustomRole.TryAdd(id, Data);
                    else
                        LogManager.Warn($"Function SpawnManager::ParseEscapeRole[2](<...>) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalTeam / IT / CustomRole / CR. Found: {Elements[2]}\n- The target is not a CustomRole / InternalRole. Found: {Elements[3]} (int32: {int.Parse(Elements[3])}");
                }
            }

            // Now let's assign
            if (!player.ReferenceHub.inventory.IsDisarmed())
                return Default;
            else if (player.ReferenceHub.inventory.IsDisarmed() && Player.Get(DisarmedPlayers.Entries.FirstOrDefault(entry => entry.DisarmedPlayer == player.NetworkId).Disarmer) is not null)
                if (Player.Get(DisarmedPlayers.Entries.FirstOrDefault(entry => entry.DisarmedPlayer == player.NetworkId).Disarmer).TryGetSummonedInstance(out SummonedCustomRole role) && AsCuffedByCustomRole.ContainsKey(role.Role.Id))
                    return AsCuffedByCustomRole[role.Role.Id];
                else if (AsCuffedByInternalTeam.ContainsKey(Player.Get(DisarmedPlayers.Entries.FirstOrDefault(entry => entry.DisarmedPlayer == player.NetworkId).Disarmer).Team))
                    return AsCuffedByInternalTeam[Player.Get(DisarmedPlayers.Entries.FirstOrDefault(entry => entry.DisarmedPlayer == player.NetworkId).Disarmer).Team];

            LogManager.Silent($"Returing default type for escaping evaluation of player {player.PlayerId} - idk if cuffed");
            return Default;
        }

        public static KeyValuePair<bool, object> ParseEscapeString(string escape)
        {
            List<string> Elements = escape.Split(' ').ToList();
            if (Elements.Count != 2)
            {
                LogManager.Warn($"Failed to parse an EscapeString[value]: syntax should be <source> <id> (2 args), found {Elements.Count} args!\nSource: {escape}");
                return new(false, RoleTypeId.Spectator);
            }

            if (Elements[0] is "CustomRole" || Elements[0] is "CR")
                return new(true, int.Parse(Elements[1]));
            else if ((Elements[0] is "InternalRole" || Elements[0] is "IR") && Enum.TryParse(Elements[1], out RoleTypeId role))
                return new(false, role);
            else
                LogManager.Warn($"Function SpawnManager::ParseEscapeString(string escape) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalRole / IR / CustomRole / CR. Found: {Elements[0]}\n- The target is not a CustomRole / InternalRole. Found: {Elements[1]} (int32: {int.Parse(Elements[1])}");

            return new(false, RoleTypeId.Spectator);
        }

#nullable enable
#pragma warning disable CS8602 // <Element> can be null at this point! (added a check!)
        public static ICustomRole? DoEvaluateSpawnForPlayer(Player player, RoleTypeId? role = null)
        {
            role ??= player.Role;

            RoleTypeId NewRole = (RoleTypeId)role;

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

            foreach (ICustomRole Role in CustomRole.CustomRoles.Values.Where(cr => cr.SpawnSettings is not null))
                if (!Role.IgnoreSpawnSystem && Player.GetPlayers().Count >= Role.SpawnSettings.MinPlayers && SummonedCustomRole.Count(Role) < Role.SpawnSettings.MaxPlayers)
                    foreach (RoleTypeId RoleType in Role.SpawnSettings.CanReplaceRoles)
                        for (int a = 0; a < Role.SpawnSettings.SpawnChance; a++)
                            RolePercentage[RoleType].Add(Role);

            LogManager.Silent($"RLC {RolePercentage.Count} ppts {RolePercentage[NewRole]?.Count} for {NewRole}");

            if (player.HasCustomRole())
            {
                LogManager.Debug("Was evalutating role select for an already custom role player, stopping");
                return null;
            }

            if (RolePercentage.ContainsKey(player.Role))
                if (UnityEngine.Random.Range(0, 100) < RolePercentage[NewRole].Count())
                    return CustomRole.CustomRoles[RolePercentage[NewRole].RandomItem().Id];

            return null;
        }
    }  
}