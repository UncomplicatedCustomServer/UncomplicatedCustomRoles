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

namespace UncomplicatedCustomRoles.Manager
{
    internal class SpawnManager
    {
        public static void RegisterCustomSubclass(ICustomRole Role, bool notLoadIfLoaded = false)
        {
            if (!SubclassValidator(Role))
            {
                LogManager.Warn($"Failed to register the UCR role with the ID {Role.Id} due to the validator check!");

                return;
            }

            if (!Plugin.CustomRoles.ContainsKey(Role.Id))
            {
                Plugin.CustomRoles.Add(Role.Id, Role);

                if (Plugin.Instance.Config.EnableBasicLogs)
                {
                    LogManager.Info($"Successfully registered the UCR role with the ID {Role.Id} and {Role.Name} as name!");
                }

                return;
            }

            if (notLoadIfLoaded)
            {
                LogManager.Debug($"Can't load role {Role.Id} {Role.Name} due to plugin settings!\nPlease reach UCS support for UCR!");
                return;
            }

            LogManager.Warn($"Failed to register the UCR role with the ID {Role.Id}: The problem can be the following: ERR_ID_ALREADY_HERE!\nTrying to assign a new one...");

            int NewId = GetFirstFreeID(Role.Id);

            LogManager.Info($"Custom Role {Role.Name} with the old Id {Role.Id} will be registered with the following Id: {NewId}");

            Role.Id = NewId;

            RegisterCustomSubclass(Role);
        }

        public static bool SubclassValidator(ICustomRole Role)
        {
            if (Role is null)
            {
                return false;
            } else
            {
                if (Role.SpawnSettings is null)
                {
                    LogManager.Warn($"Is kinda useless registering a role with no spawn_settings.\nFound (or not found) in role: {Role.Name} ({Role.Id})");
                    return false;
                }

                if (Role.SpawnSettings.Spawn == SpawnLocationType.ZoneSpawn && Role.SpawnSettings.SpawnZones.Count() < 1)
                {
                    LogManager.Warn($"The UCR custom role with the ID {Role.Id} failed the check: if you select the ZoneSpawn as SpawnType the List SpawnZones can't be empty!");
                    return false;
                } else if (Role.SpawnSettings.Spawn == SpawnLocationType.RoomsSpawn && Role.SpawnSettings.SpawnRooms.Count() < 1)
                {
                    LogManager.Warn($"The UCR custom role with the ID {Role.Id} failed the check: if you select the RoomSpawn as SpawnType the List SpawnRooms can't be empty!");
                    return false;
                } else if (Role.SpawnSettings.Spawn == SpawnLocationType.PositionSpawn && Role.SpawnSettings.SpawnPosition == new Vector3(0, 0, 0))
                {
                    LogManager.Warn($"The UCR custom role with the ID {Role.Id} failed the check: if you select the PositionSpawn as SpawnType the Vector3 SpawnPosition can't be empty!");
                    return false;
                }
                else if (Role.SpawnSettings.MinPlayers == 0)
                {
                    LogManager.Warn($"The UCR custom role with the ID {Role.Id} failed the check: the value of MinPlayers field must be greater than or equals to 1!");
                    return false;
                }
                return true;
            }
        }

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
            }

            player.CustomInfo = string.Empty;
            player.Scale = new(1, 1, 1);
            LimitedClearCustomTypes(player);
        }

        public static void LimitedClearCustomTypes(Player player)
        {
            if (player.TryGetCustomRole(out ICustomRole role))
            {
                player.IsUsingStamina = true;
                Plugin.PermanentEffectStatus.Remove(player.Id);
                Plugin.RolesCount[role.Id].Remove(player.Id);
                Plugin.PlayerRegistry.Remove(player.Id);
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

            Vector3 BasicPosition = Player.Position;

            RoleSpawnFlags SpawnFlag = RoleSpawnFlags.None;

            if (Role.SpawnSettings.Spawn == SpawnLocationType.KeepRoleSpawn)
            {
                SpawnFlag = RoleSpawnFlags.UseSpawnpoint;
            }

            Player.Role.Set(Role.Role, SpawnFlag);

            if (Role.SpawnSettings.Spawn == SpawnLocationType.KeepCurrentPositionSpawn)
            {
                Player.Position = BasicPosition;
            }

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
                        {
                            Player.Position += Role.SpawnSettings.SpawnOffset;
                        }
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
            {
                foreach (uint ItemId in Role.CustomItemsInventory)
                {
                    if (!Player.IsInventoryFull)
                    {
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
                            {
                                CustomItem.Get(ItemId)?.Give(Player);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogManager.Debug($"Error while giving a custom item.\nError: {ex.Message}");
                        }
                    }
                }
            }

            Player.IsUsingStamina = !Role.InfiniteStamina;

            if (Role.Ammo.GetType() == typeof(Dictionary<AmmoType, ushort>) && Role.Ammo.Count() > 0)
            {
                foreach (KeyValuePair<AmmoType, ushort> Ammo in Role.Ammo)
                {
                    Player.AddAmmo(Ammo.Key, Ammo.Value);
                }
            }

            if (Role.CustomInfo != null && Role.CustomInfo != string.Empty)
            {
                Player.CustomInfo += $"\n{Role.CustomInfo}";
            }

            Player.MaxHealth = Role.MaxHealth;
            Player.Health = Role.Health;
            Player.ArtificialHealth = Role.Ahp;

            Plugin.PermanentEffectStatus.Add(Player.Id, new());

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

            // Add the player to the player classes list
            Plugin.PlayerRegistry.Add(Player.Id, Role.Id);
            Plugin.RolesCount[Role.Id].Add(Player.Id);

            if (Role.HumeShield > 0)
            {
                Player.HumeShield = Role.HumeShield;
            }

            if (Role.Scale != new Vector3(0, 0, 0))
            {
                Player.Scale = Role.Scale;
            }

            if (Role.SpawnBroadcast != string.Empty)
            {
                Player.Broadcast(Role.SpawnBroadcastDuration, Role.SpawnBroadcast);
            }

            if (Role.SpawnHint != string.Empty)
            {
                Player.ShowHint(Role.SpawnHint, Role.SpawnHintDuration);
            }

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

            if (Role.RoleAppearance != Role.Role)
            {
                LogManager.Debug($"Changing the appearance of the role {Role.Id} [{Role.Name}] to {Role.RoleAppearance}");
                Timing.CallDelayed(1f, () =>
                {
                    Player.ChangeAppearance(Role.RoleAppearance, true);
                });
            }
        }
        
        public static int GetFirstFreeID(int Id)
        {
            while (Plugin.CustomRoles.ContainsKey(Id))
            {
                Id++;
            }
            return Id;
        }

        public static void SetAllActiveEffect(Player Player)
        {
            foreach (IUCREffect Effect in Plugin.PermanentEffectStatus[Player.Id])
            {
                Player.EnableEffect(Effect.EffectType, 15f);
                Player.ChangeEffectIntensity(Effect.EffectType, Effect.Intensity, 15f);
            }
        }

        public static RoleTypeId? ParseEscapeRole(string roleAfterEscape, Player player)
        {
            if (roleAfterEscape is not null && roleAfterEscape != string.Empty)
            {
                // Syntax: IR (Internal Role) or CR (Custom Role) : the ID.   For example IR:0 will be SCP-173 (also IR:Scp173) and CR:1 will be the Custom Role with the Id = 1
                string[] Action = roleAfterEscape.Split(':');
                if (Action[0].ToUpper() == "IR")
                {
                    if (Enum.TryParse(Action[1], out RoleTypeId Out))
                    {
                        return Out;
                    }
                }
                else if (Action[0].ToUpper() == "CR")
                {
                    LogManager.Debug($"Start parsing the action for a custom role. Full: {roleAfterEscape}");
                    if (int.TryParse(Action[1], out int Id))
                    {
                        LogManager.Debug($"Found a valid Id (i guess so): {Id}");
                        if (Plugin.CustomRoles.ContainsKey(Id))
                        {
                            LogManager.Debug($"Seems that the role {Id} really exists, let's gooo!");
                            SummonCustomSubclass(player, Id, true);
                        }
                    }
                }
            }
            return null;
        }
    }  
}