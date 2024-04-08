using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Structures;
using UnityEngine;
using UncomplicatedCustomRoles.Elements;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Features.Items;
using InventorySystem.Items.Firearms.Attachments;

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

        public static void RegisterCustomFirearm(ICustomFirearm Firearm)
        {
            if (!Plugin.Firearms.ContainsKey(Firearm.Id))
            {
                Plugin.Firearms.Add(Firearm.Id, Firearm);
            } 
            else
            {
                Log.Warn($"Falied to register the UCR Custom Firearm with the Id {Firearm.Id} ({Firearm.Item}) -> There's another role with this Id!");
            }
        }

        public static ICustomRole RenderExportMethodToInternal(IExternalCustomRole Role)
        {
            return new CustomRole()
            {
                Name = Role.Name,
                Id = Role.Id,
                CustomInfo = Role.CustomInfo,
                MaxHealth = Role.MaxHealth,
                MaxPlayers = Role.MaxPlayers,
                MinPlayers = Role.MinPlayers,
                Role = Role.Role,
                RoleAppearance = Role.RoleAppearance,
                CanReplaceRoles = Role.CanReplaceRoles,
                Ahp = Role.Ahp,
                HumeShield = Role.HumeShield,
                Effects = Role.Effects,
                CanEscape = Role.CanEscape,
                Scale = VectorConvertor(Role.Scale),
                SpawnBroadcast = Role.SpawnBroadcast,
                SpawnBroadcastDuration = Role.SpawnBroadcastDuration,
                SpawnHint = Role.SpawnHint,
                SpawnHintDuration = Role.SpawnHintDuration,
                Inventory = Role.Inventory,
                CustomItemsInventory = Role.CustomItemsInventory,
                Ammo = Role.Ammo,
                Spawn = Role.Spawn,
                SpawnRooms = Role.SpawnRooms,
                SpawnZones = Role.SpawnZones,
                SpawnPosition = VectorConvertor(Role.SpawnPosition),
                SpawnOffset = VectorConvertor(Role.SpawnOffset),
                RequiredPermission = Role.RequiredPermission,
                IgnoreSpawnSystem = Role.IgnoreSpawnSystem,
                Health = Role.Health,
                SpawnChance = Role.SpawnChance,
                DamageMultiplier = Role.DamageMultiplier,
                RoleAfterEscape = Role.RoleAfterEscape,
                InfiniteStamina = Role.InfiniteStamina
            };
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

        public static Firearm CreateFirearm(ICustomFirearm Firearm)
        {
            Firearm Item = Exiled.API.Features.Items.Firearm.Create(Firearm.Item);

            foreach (AttachmentName Attachment in Firearm.Attachments)
            {
                Item.AddAttachment(Attachment);
            }

            if (Firearm.Scale != null && Firearm.Scale != new Vector3())
            {
                Item.Scale = Firearm.Scale;
            }

            if (Firearm.MaxAmmo is not null && Firearm.MaxAmmo > 0)
            {
                Item.MaxAmmo = (byte)Firearm.MaxAmmo;
            }

            if (Firearm.FireRate is not null && Firearm.FireRate > 0)
            {
                Item.FireRate = (float)Firearm.FireRate;
            }

            return Item;
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
                        Player.Position = Factory.AdjustRoomPosition(Factory.RoomsInZone(Role.SpawnZones.RandomItem()).RandomItem());
                        break;
                    case SpawnLocationType.CompleteRandomSpawn:
                        Player.Position = Factory.AdjustRoomPosition(Room.List.ToList().RandomItem());
                        break;
                    case SpawnLocationType.RoomsSpawn:
                        Player.Position = Factory.AdjustRoomPosition(Room.Get(Role.SpawnRooms.RandomItem()));
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
                        // If the Id is the Id of a registered custom weapon we'll give it, otherwise the custom item will be given
                        if (Plugin.Firearms.ContainsKey((int)ItemId))
                        {
                            CreateFirearm(Plugin.Firearms[(int)ItemId]).Give(Player);
                        } 
                        else
                        {
                            CustomItem.Get(ItemId)?.Give(Player);
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

            // Call the event for the spawn of the player
            API.Features.Events.__CallEvent(UCREvents.Spawned, new SpawnedEventArgs(Player, OldRole));
        }

        public static Vector3 VectorConvertor(string Vector)
        {
            if (Vector.Length - Vector.Replace(",", "").Length != 2)
            {
                Log.Warn($"Error while parsing StringVector '{Vector}', found {Vector.Length - Vector.Replace(",", "").Length} commas instead of 2!\nSyntax: x, y, z");
                return new();
            }
            string[] Data = Vector.Replace(" ", "").Split(',');
            if (Data.Length != 3) 
            {
                Log.Warn($"Error while parsing StringVector '{Vector}', found {Data.Length} elements instead of 3!\nSyntax: x, y, z");
                return new();
            }
            Vector3 Vector3 = new(float.Parse(Data[0].Replace(".", ",")), float.Parse(Data[1].Replace(".", ",")), float.Parse(Data[2].Replace(".", ",")));
            Log.Debug($"Parsed StringVector '{Vector}' with success -- Results: {Vector3}!");
            return Vector3;
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
    }  
}