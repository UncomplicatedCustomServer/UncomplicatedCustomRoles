using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Interfaces;
using UnityEngine;
using Exiled.CustomItems.API.Features;
using System;
using UncomplicatedCustomRoles.Extensions;
using MEC;
using Exiled.Permissions.Extensions;

// Mormora, la gente mormora
// falla tacere praticando l'allegria

namespace UncomplicatedCustomRoles.Manager
{
    internal class SpawnManager
    {
        public static void ClearCustomTypes(Player player)
        {
            if (player.TryGetCustomRole(out ICustomRole role))
            {
                if (role.BadgeName is not null && role.BadgeName.Length > 1 && role.BadgeColor is not null && role.BadgeColor.Length > 2 && Plugin.Tags.ContainsKey(player.Id))
                {
                    player.RankName = Plugin.Tags[player.Id][0];
                    player.RankColor = Plugin.Tags[player.Id][1];

                    LogManager.Debug($"Badge detected, fixed");

                    Plugin.Tags.Remove(player.Id);
                }

                player.IsUsingStamina = true;
                Plugin.PermanentEffectStatus.Remove(player.Id);
                Plugin.RolesCount[role.Id].Remove(player.Id);
                Plugin.PlayerRegistry.Remove(player.Id);
            }

            Plugin.Scp330Count.TryRemove(player.Id);

            player.CustomInfo = string.Empty;

            LogManager.Debug("Scale reset to 1, 1, 1");
            player.Scale = new(1, 1, 1);
            
            if (Plugin.NicknameTracker.Contains(player.Id))
            {
                player.DisplayNickname = null;
                Plugin.NicknameTracker.Remove(player.Id);
            }
        }

        public static void SummonCustomSubclass(Player Player, int Id, bool DoBypassRoleOverwrite = true)
        {
            // Does the role exists?
            if (!Plugin.CustomRoles.ContainsKey(Id))
            {
                LogManager.Warn($"Sorry but the role with the Id {Id} is not registered inside UncomplicatedCustomRoles!");
                return;
            }

            ICustomRole Role = Plugin.CustomRoles[Id];

            if (Role.SpawnSettings is null)
            {
                LogManager.Warn($"Tried to spawn a custom role without spawn_settings, aborting the SummonCustomSubclass(...) action!\nRole: {Role.Name} ({Role.Id})");
                return;
            }

            if (!DoBypassRoleOverwrite && !Role.SpawnSettings.CanReplaceRoles.Contains(Player.Role.Type))
            {
                LogManager.Debug($"Can't spawn the player {Player.Nickname} as UCR custom role {Role.Name} because it's role is not in the overwrittable list of custom role!\nStrange because this should be managed correctly by the plugin!");
                return;
            }

            Plugin.InternalCooldownQueue.Add(Player.Id);

            Vector3 BasicPosition = Player.Position;

            RoleSpawnFlags SpawnFlag = RoleSpawnFlags.None;

            if (Role.SpawnSettings.Spawn == SpawnLocationType.KeepRoleSpawn)
                SpawnFlag = RoleSpawnFlags.UseSpawnpoint;

            // To avoid the loop on OnChangingRole we just add the Id inside the beautiful array!
            // Add the player to the player classes list
            Plugin.RolesCount[Role.Id].Add(Player.Id);
            Plugin.PlayerRegistry.Add(Player.Id, Role.Id);
            Plugin.PermanentEffectStatus.Add(Player.Id, new());

            Player.Role.Set(Role.Role, SpawnFlag);

            if (Role.SpawnSettings.Spawn == SpawnLocationType.KeepCurrentPositionSpawn)
                Player.Position = BasicPosition;

            if (SpawnFlag == RoleSpawnFlags.None)
            {
                switch (Role.SpawnSettings.Spawn)
                {
                    case SpawnLocationType.ZoneSpawn:
                        Player.Position = Room.List.Where(room => room.Zone == Role.SpawnSettings.SpawnZones.RandomItem() && room.TeslaGate is null).GetRandomValue().Position.AddY(1.5f);
                        break;
                    case SpawnLocationType.CompleteRandomSpawn:
                        Player.Position = Room.List.Where(room => room.TeslaGate is null).GetRandomValue().Position.AddY(1.5f);
                        break;
                    case SpawnLocationType.RoomsSpawn:
                        Player.Position = Room.Get(Role.SpawnSettings.SpawnRooms.RandomItem()).Position.AddY(1.5f);

                        if (Role.SpawnSettings.SpawnOffset != new Vector3())
                            Player.Position += Role.SpawnSettings.SpawnOffset;

                        break;
                    case SpawnLocationType.PositionSpawn:
                        Player.Position = Role.SpawnSettings.SpawnPosition;
                        break;
                };
            }

            SummonSubclassApplier(Player, Role);
        }

        public static void SummonSubclassApplier(Player Player, ICustomRole Role)
        {
            Player.ResetInventory(Role.Inventory);

            if (Role.CustomItemsInventory.Count() > 0)
                foreach (uint ItemId in Role.CustomItemsInventory)
                    if (!Player.IsInventoryFull)
                        try
                        {
                            if (Exiled.Loader.Loader.GetPlugin("UncomplicatedCustomItems") is not null)
                            {
                                Type AssemblyType = Exiled.Loader.Loader.GetPlugin("UncomplicatedCustomItems").Assembly.GetType("UncomplicatedCustomItems.API.Utilities");
                                if ((bool)AssemblyType?.GetMethod("IsCustomItem")?.Invoke(null, new object[] { ItemId }))
                                {
                                    object CustomItem = AssemblyType?.GetMethod("GetCustomItem")?.Invoke(null, new object[] { ItemId });

                                    Exiled.Loader.Loader.GetPlugin("UncomplicatedCustomItems").Assembly.GetType("UncomplicatedCustomItems.API.Features.SummonedCustomItem")?.GetMethods().Where(method => method.Name == "Summon" && method.GetParameters().Length == 2).FirstOrDefault()?.Invoke(null, new object[]
                                    {
                                        CustomItem,
                                        Player
                                    });
                                }
                            }
                            else
                                CustomItem.Get(ItemId)?.Give(Player);
                        }
                        catch (Exception ex)
                        {
                            LogManager.Debug($"Error while giving a custom item.\nError: {ex.Message}");
                        }

            if (Role.Ammo.GetType() == typeof(Dictionary<AmmoType, ushort>) && Role.Ammo.Count() > 0)
                foreach (KeyValuePair<AmmoType, ushort> Ammo in Role.Ammo)
                    Player.AddAmmo(Ammo.Key, Ammo.Value);

            if (Role.CustomInfo != null && Role.CustomInfo != string.Empty)
                Player.CustomInfo += $"\n{Role.CustomInfo}";

            if (!Plugin.RolesCount[Role.Id].Contains(Player.Id))
            {
                Plugin.RolesCount[Role.Id].Add(Player.Id);
                Plugin.PlayerRegistry.Add(Player.Id, Role.Id);
                Plugin.PermanentEffectStatus.Add(Player.Id, new());
            }

            // Apply every required stats
            Role.Health?.Apply(Player);
            Role.Ahp?.Apply(Player);
            Role.Stamina?.Apply(Player);

            if (Role.Effects.Count() > 0 && Role.Effects != null)
            {
                foreach (IUCREffect effect in Role.Effects)
                {
                    float Duration = effect.Duration;
                    if (Duration < 0)
                    {
                        Duration = 15f;
                        Plugin.PermanentEffectStatus[Player.Id].Add(effect);
                    }
                    Player.EnableEffect(effect.EffectType, Duration);
                    Player.ChangeEffectIntensity(effect.EffectType, effect.Intensity, Duration);
                }
            }

            if (Role.Scale != Vector3.zero && Role.Scale != Vector3.one)
                Player.Scale = Role.Scale;

            if (Role.SpawnBroadcast != string.Empty)
            {
                Player.ClearBroadcasts();
                Player.Broadcast(Role.SpawnBroadcastDuration, Role.SpawnBroadcast);
            }

            if (Role.SpawnHint != string.Empty)
                Player.ShowHint(Role.SpawnHint, Role.SpawnHintDuration);

            if (Role.BadgeName is not null && Role.BadgeName.Length > 1 && Role.BadgeColor is not null && Role.BadgeColor.Length > 2)
            {
                Plugin.Tags.Add(Player.Id, new string[]
                {
                        Player.RankName ?? "",
                        Player.RankColor ?? ""
                });

                LogManager.Debug($"Badge detected, putting {Role.BadgeName}@{Role.BadgeColor} to player {Player.Id}");

                Player.RankName = Role.BadgeName;
                Player.RankColor = Role.BadgeColor;
            }

            // Changing nickname if needed
            if (Plugin.Instance.Config.AllowNicknameEdit && Role.Nickname is not null && Role.Nickname != string.Empty)
            {
                Role.Nickname = Role.Nickname.Replace("%dnumber%", new System.Random().Next(1000, 9999).ToString()).Replace("%nick%", Player.Nickname).Replace("%rand%", new System.Random().Next(0, 9).ToString());
                if (Role.Nickname.Contains(","))
                    Player.DisplayNickname = Role.Nickname.Split(',').RandomItem();
                else
                    Player.DisplayNickname = Role.Nickname;

                Plugin.NicknameTracker.Add(Player.Id);
            }

            if (Role.RoleAppearance != Role.Role)
            {
                LogManager.Debug($"Changing the appearance of the role {Role.Id} [{Role.Name}] to {Role.RoleAppearance}");
                Timing.CallDelayed(1f, () =>
                {
                    Player.ChangeAppearance(Role.RoleAppearance, true);
                });
            }

            LogManager.Debug($"{Player} successfully spawned as {Role.Name} ({Role.Id})!");
        }

        public static void SetAllActiveEffect(Player Player)
        {
            foreach (IUCREffect Effect in Plugin.PermanentEffectStatus[Player.Id])
            {
                Player.EnableEffect(Effect.EffectType, 15f);
                Player.ChangeEffectIntensity(Effect.EffectType, Effect.Intensity, 15f);
            }
        }

        public static KeyValuePair<bool, object> ParseEscapeRole(string roleAfterEscape, Player player)
        {
            List<string> Role = new();

            if (roleAfterEscape is not null && roleAfterEscape != string.Empty)
            {
                if (roleAfterEscape.Contains(","))
                {
                    string[] roles = roleAfterEscape.Split(',');
                    foreach (string role in roles)
                        foreach (string rolePart in role.Split(':')) 
                            Role.Add(rolePart);
                }

                int SearchIndex = 0;

                if (player.IsCuffed && player.Cuffer is not null)
                    SearchIndex = player.Cuffer.Role.Team switch
                    {
                        Team.FoundationForces => 2,
                        Team.ChaosInsurgency => 4,
                        Team.Scientists => 6,
                        Team.ClassD => 8,
                        _ => 0
                    };

                // Let's proceed
                if (Role.Count >= SearchIndex + 2)
                    if (Role[SearchIndex] is "IR")
                        return new(false, Role[SearchIndex + 1]);
                    else if (Role[SearchIndex] is "CR")
                        return new(true, Role[SearchIndex + 1]);
                    else
                        LogManager.Error($"Error while parsing role_after_escape for player {player.Nickname} ({player.Id}): the first string was not 'IR' nor 'CR', found '{Role[SearchIndex]}'!\nPlease see our documentation: https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/wiki/Specifics#role-after-escape");
                else
                    LogManager.Debug($"Error while parsing role_after_escape: index is out of range!\nExpected to found {SearchIndex}, total: {Role.Count}!");
            }

            return new(false, null);
        }

#nullable enable
#pragma warning disable CS8602 // <Element> can be null at this point! (added a check!)
        public static ICustomRole? DoEvaluateSpawnForPlayer(Player player, RoleTypeId? role = null)
        {
            role ??= player.Role.Type;

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

            foreach (ICustomRole Role in Plugin.CustomRoles.Values.Where(cr => cr.SpawnSettings is not null))
                if (!Role.IgnoreSpawnSystem && Player.List.Count() >= Role.SpawnSettings.MinPlayers)
                {
                    if (Role.SpawnSettings.RequiredPermission != null && Role.SpawnSettings.RequiredPermission != string.Empty && !player.CheckPermission(Role.SpawnSettings.RequiredPermission))
                    {
                        LogManager.Debug($"[NOTICE] Ignoring the role {Role.Id} [{Role.Name}] while creating the list for the player {player.Nickname} due to: cannot [permissions].");
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

            if (Plugin.PlayerRegistry.ContainsKey(player.Id))
            {
                LogManager.Debug("Was evalutating role select for an already custom role player, stopping");
                return null;
            }

            if (RolePercentage.ContainsKey(player.Role.Type))
                if (new System.Random().Next(0, 100) < RolePercentage[NewRole].Count())
                {
                    // The role exists, good, let's give the player a role
                    int RoleId = RolePercentage[NewRole].RandomItem().Id;

                    if (Plugin.CustomRoles[RoleId] is null || Plugin.CustomRoles[RoleId].SpawnSettings is null)
                        return Plugin.CustomRoles[RoleId];

                    if (Plugin.RolesCount[RoleId].Count() <= Plugin.CustomRoles[RoleId].SpawnSettings.MaxPlayers)
                        return Plugin.CustomRoles[RoleId];
                    else
                        LogManager.Debug($"Player {player.Nickname} won't be spawned as CustomRole {RoleId} because it has reached the maximus number");
                }

            return null;
        }
    }  
}