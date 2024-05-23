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
using Exiled.API.Interfaces;
using InventorySystem.Items;
using System.Reflection;

namespace UncomplicatedCustomRoles.Manager
{
    public class SpawnManager
    {
        public static void RegisterCustomSubclass(ICustomRole Role)
        {
            if (!SubclassValidator(Role))
            {
                Log.Warn($"Failed to register the UCR role with the ID {Role.Id} due to the validator check!");

                return;
            }

            if (!Plugin.CustomRoles.ContainsKey(Role.Id))
            {
                Plugin.CustomRoles.Add(Role.Id, Role);

                if (Plugin.Instance.Config.EnableBasicLogs)
                {
                    Log.Info($"Successfully registered the UCR role with the ID {Role.Id} and {Role.Name} as name!");
                }

                return;
            }

            Log.Warn($"Failed to register the UCR role with the ID {Role.Id}: The problem can be the following: ERR_ID_ALREADY_HERE!\nTrying to assign a new one...");

            int NewId = GetFirstFreeID(Role.Id);

            Log.Info($"Custom Role {Role.Name} with the old Id {Role.Id} will be registered with the following Id: {NewId}");

            Role.Id = NewId;

            RegisterCustomSubclass(Role);
        }

        public static bool SubclassValidator(ICustomRole Role)
        {
            if (Role == null)
            {
                return false;
            } else
            {
                if (Role.Spawn == SpawnLocationType.ZoneSpawn && Role.SpawnZones.Count() < 1)
                {
                    Log.Warn($"The UCR custom role with the ID {Role.Id} failed the check: if you select the ZoneSpawn as SpawnType the List SpawnZones can't be empty!");
                    return false;
                } else if (Role.Spawn == SpawnLocationType.RoomsSpawn && Role.SpawnRooms.Count() < 1)
                {
                    Log.Warn($"The UCR custom role with the ID {Role.Id} failed the check: if you select the RoomSpawn as SpawnType the List SpawnRooms can't be empty!");
                    return false;
                } else if (Role.Spawn == SpawnLocationType.PositionSpawn && Role.SpawnPosition == new Vector3(0, 0, 0))
                {
                    Log.Warn($"The UCR custom role with the ID {Role.Id} failed the check: if you select the PositionSpawn as SpawnType the Vector3 SpawnPosition can't be empty!");
                    return false;
                }
                else if (Role.MinPlayers == 0)
                {
                    Log.Warn($"The UCR custom role with the ID {Role.Id} failed the check: the value of MinPlayers field must be greater than or equals to 1!");
                    return false;
                }
                return true;
            }
        }

        public static void ClearCustomTypes(Player Player)
        {
            Player.CustomInfo = string.Empty;
            Player.Scale = new(1, 1, 1);
            if (Plugin.PlayerRegistry.ContainsKey(Player.Id))
            {
                Plugin.PermanentEffectStatus.Remove(Player.Id);
                int Role = Plugin.PlayerRegistry[Player.Id];
                Plugin.RolesCount[Role].Remove(Player.Id);
                Plugin.PlayerRegistry.Remove(Player.Id);
            }
        }

        public static void LimitedClearCustomTypes(Player Player)
        {
            if (Plugin.PlayerRegistry.ContainsKey(Player.Id))
            {
                Plugin.PermanentEffectStatus.Remove(Player.Id);
                int Role = Plugin.PlayerRegistry[Player.Id];
                Plugin.RolesCount[Role].Remove(Player.Id);
                Plugin.PlayerRegistry.Remove(Player.Id);
            }
        }

        public static void SummonCustomSubclass(Player Player, int Id, bool DoBypassRoleOverwrite = true)
        {
            // Does the role exists?
            if (!Plugin.CustomRoles.ContainsKey(Id))
            {
                Log.Warn($"Sorry but the role with the Id {Id} is not registered inside UncomplicatedCustomRoles!");
                return;
            }

            ICustomRole Role = Plugin.CustomRoles[Id];

            if (!DoBypassRoleOverwrite && !Role.CanReplaceRoles.Contains(Player.Role.Type))
            {
                Log.Debug($"Can't spawn the player {Player.Nickname} as UCR custom role {Role.Name} because it's role is not in the overwrittable list of custom role!\nStrange because this should be managed correctly by the plugin!");
                return;
            }

            RoleSpawnFlags SpawnFlag = RoleSpawnFlags.None;

            if (Role.Spawn == SpawnLocationType.KeepRoleSpawn)
            {
                SpawnFlag = RoleSpawnFlags.UseSpawnpoint;
            }
            // It's all OK so we can start to elaborate the spawn
            PlayerRoleBase OldRole = Player.Role.Base; // That's for the event system, don't worry!;

            Player.Role.Set(Role.Role, SpawnFlag);

            Vector3 BasicPosition = Player.Position;

            if (Role.Spawn == SpawnLocationType.KeepRoleSpawn)
            {
                Player.Position = BasicPosition;
            }

            if (SpawnFlag == RoleSpawnFlags.None)
            {
                switch (Role.Spawn)
                {
                    case SpawnLocationType.ZoneSpawn:
                        Player.Position = Room.List.Where(room => room.Zone == Role.SpawnZones.RandomItem()).GetRandomValue().Position.AddY(1.5f);
                        break;
                    case SpawnLocationType.CompleteRandomSpawn:
                        Player.Position = Room.List.GetRandomValue().Position.AddY(1.5f);
                        break;
                    case SpawnLocationType.RoomsSpawn:
                        Player.Position = Room.Get(Role.SpawnRooms.RandomItem()).Position.AddY(1.5f);
                        if (Role.SpawnOffset != new Vector3())
                        {
                            Player.Position += Role.SpawnOffset;
                        }
                        break;
                    case SpawnLocationType.PositionSpawn:
                        Player.Position = Role.SpawnPosition;
                        break;
                };
            }

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
                            Log.Debug($"Error while giving a custom item.\nError: {ex.Message}");
                        }
                    }
                }
            }
            
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

            Player.Health = Role.Health;
            Player.MaxHealth = Role.MaxHealth;
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

            if (Role.RoleAppearance != Role.Role)
            {
                Log.Debug($"Changing the appearance of the role {Role.Id} [{Role.Name}] to {Role.RoleAppearance}");
                Player.ChangeAppearance(Role.RoleAppearance, true);
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

        public static int? TryGetCustomRole(Player Player)
        {
            if (Plugin.PlayerRegistry.ContainsKey(Player.Id))
            {
                return Plugin.PlayerRegistry[Player.Id];
            }
            return null;
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
                    Log.Debug($"Start parsing the action for a custom role. Full: {roleAfterEscape}");
                    if (int.TryParse(Action[1], out int Id))
                    {
                        Log.Debug($"Found a valid Id (i guess so): {Id}");
                        if (Plugin.CustomRoles.ContainsKey(Id))
                        {
                            Log.Debug($"Seems that the role {Id} really exists, let's gooo!");
                            if (!player.IsScp)
                            {
                                Timing.CallDelayed(2f, () =>
                                {
                                    Timing.RunCoroutine(Events.EventHandler.DoSpawnPlayer(player, Id, true));
                                });
                            }
                            else
                            {
                                Timing.RunCoroutine(Events.EventHandler.DoSpawnPlayer(player, Id, true));
                            }
                        }
                    }
                }
            }
            return null;
        }
    }  
}