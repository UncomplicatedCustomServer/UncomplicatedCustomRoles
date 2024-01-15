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

namespace UncomplicatedCustomRoles.Manager
{
    public class SpawnManager
    {
        public static void RegisterCustomSubclass(ICustomRole Role)
        {
            if (SubclassValidator(Role) && !Plugin.CustomRoles.ContainsKey(Role.Id))
            {
                Plugin.CustomRoles.Add(Role.Id, Role);
                Plugin.RolesCount.Add(Role.Id, 0);
                Log.Info($"Successfully registered the UCR role with the ID {Role.Id} and {Role.Name} as name!");
                return;
            }
            Log.Warn($"Failed to register the UCR role with the ID {Role.Id}: The problem can be the following: ERR_VALIDATOR or ERR_ID_ALREADY_HERE!\nTrying to assign a new one...");
            int NewId = GetFirstFreeID(Role.Id);
            Log.Info($"Custom Role {Role.Name} with the old Id {Role.Id} will be registered with the following Id: {NewId}");
            Role.Id = NewId;
            RegisterCustomSubclass(Role);
        }

        public static ICustomRole RenderExportMethodToInternal(IExternalCustomRole Role)
        {
            return new CustomRole()
            {
                Name = Role.Name,
                Id = Role.Id,
                CustomInfo = Role.CustomInfo,
                DisplayNickname = Role.DisplayNickname,
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
                SpawnChance = Role.SpawnChance
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
                } else if (Role.Spawn == SpawnLocationType.PositionSpawn && Role.SpawnPosition == new UnityEngine.Vector3(0, 0, 0))
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
            if (Plugin.PlayerRegistry.ContainsKey(Player.Id))
            {
                Plugin.PlayerRegistry.Remove(Player.Id);
                Player.CustomInfo = string.Empty;
                Player.DisplayNickname = string.Empty;
                Player.Scale = new Vector3(1, 1, 1);
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
            Player.Role.Set(Role.Role, SpawnFlag);
            Vector3 BasicPosition = Player.Position;

            if (Role.RoleAppearance != Role.Role)
            {
                Log.Debug($"Changing the appearance of the role {Role.Id} [{Role.Name}] to {Role.RoleAppearance}");
                Player.ChangeAppearance(Role.RoleAppearance, true);
            }

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
                        Player.Position = Factory.AdjustRoomPosition(Factory.GetRoomList().RandomItem());
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
                        CustomItem.Get(ItemId).Give(Player);
                    }
                }
            }
            // Now add all ammos
            if (Role.Ammo.GetType() == typeof(Dictionary<AmmoType, ushort>) && Role.Ammo.Count() > 0)
            {
                foreach (KeyValuePair<AmmoType, ushort> Ammo in Role.Ammo)
                {
                    Player.AddAmmo(Ammo.Key, Ammo.Value);
                }
            }
            // Player.Group.BadgeColor = Role.Badge.Color;
            // Player.Group.BadgeText = Role.Badge.Name;
            // Player.Group.Permissions = Player.Group.Permissions;
            Player.CustomInfo = Role.Name + ("\n" + Role.CustomInfo ?? string.Empty);
            if (Role.DisplayNickname != string.Empty && Role.DisplayNickname != null)
            {
                Player.DisplayNickname = Role.DisplayNickname.Replace("%name%", Player.Nickname).Replace("%dnumber%", new System.Random().Next(1000, 9999).ToString()).Replace("%o5number%", new System.Random().Next(01, 10).ToString());
            }

            Player.MaxHealth = Role.MaxHealth;
            Player.Health = Role.Health;
            Player.ArtificialHealth = Role.Ahp;

            if (Role.Effects.Count() > 0 && Role.Effects != null)
            {
                foreach (IUCREffect effect in Role.Effects)
                {
                    Player.EnableEffect(effect.EffectType, effect.Duration);
                    Player.ChangeEffectIntensity(effect.EffectType, effect.Intensity);
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
        }

        public static Vector3 VectorConvertor(string Vector)
        {
            string[] Data = Vector.Replace(" ", "").Split(',');
            return new Vector3(float.Parse(Data.ElementAt(0)), float.Parse(Data.ElementAt(1)), float.Parse(Data.ElementAt(2)));
        }
        
        public static int GetFirstFreeID(int Id)
        {
            while (Plugin.CustomRoles.ContainsKey(Id))
            {
                Id++;
            }
            return Id;
        }
    }  
}