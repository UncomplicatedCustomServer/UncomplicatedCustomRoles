using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UncomplicatedCustomRoles.Structures;
using UnityEngine.Android;
using UnityEngine.Rendering.HighDefinition;

namespace UncomplicatedCustomRoles.Manager
{
    public class SpawnManager
    {
        public static void RegisterCustomSubclass(ICustomRole Role)
        {
            if (SubclassValidator(Role))
            {
                Plugin.CustomRoles.Add(Role.Id, Role);
                Log.Debug($"Successfully registered the UCR role with the ID {Role.Id} and {Role.Name} as name!");
                return;
            }
            Log.Debug($"Falied to register the UCR role with the ID {Role.Id}!");
        }
        public static bool SubclassValidator(ICustomRole Role)
        {
            if (Role == null)
            {
                return false;
            } else
            {
                if (Role.Spawn == SpawnType.ZoneSpawn && Role.SpawnZones.Count() < 1)
                {
                    Log.Debug($"The UCR custom role with the ID {Role.Id} falied the check: if you select the ZoneSpawn as SpawnType the List SpawnZones can't be empty!");
                    return false;
                } else if (Role.Spawn == SpawnType.RoomsSpawn && Role.SpawnRooms.Count() < 1)
                {
                    Log.Debug($"The UCR custom role with the ID {Role.Id} falied the check: if you select the RoomSpawn as SpawnType the List SpawnRooms can't be empty!");
                    return false;
                }
                return true;
            }
        }
        public static void SummonCustomSubclass(Player Player, int Id)
        {
            // Does the role exists?
            if (!Plugin.CustomRoles.ContainsKey(Id))
            {
                Log.Debug($"Sorry but the role with the Id {Id} is not registered inside UncomplicatedCustomRoles!");
                return;
            }

            ICustomRole Role = Plugin.CustomRoles[Id];

            if (!Role.CanReplaceRoles.Contains(Player.Role.Type))
            {
                Log.Debug($"Can't spawn the player {Player.Nickname} as UCR custom role {Role.Name} because it's role is not in the overwrittable list of custom role!\nStrange because this should be managed correctly by the plugin!");
                return;
            }

            // Ok, it's all OK so we can start to elaborate the spawn
            Player.Role.Set(Role.Role, PlayerRoles.RoleSpawnFlags.None);
            switch (Role.Spawn)
            {
                case SpawnType.ZoneSpawn:
                    Player.Position = Factory.RoomsInZone(Role.SpawnZones.RandomItem()).RandomItem().Position;
                    break;
                case SpawnType.CompleteRandomSpawn:
                    Player.Position = Factory.GetRoomList().RandomItem().Position;
                    break;
                case SpawnType.RoomsSpawn:
                    Player.Position = Room.Get(Role.SpawnRooms.RandomItem()).Position;
                    break;
            };
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
            Player.CustomInfo = Role.Name;
            Player.Broadcast(Role.SpawnBroadcastDuration, Role.SpawnBroadcast);
            // Add the player to the player classes list
            Plugin.PlayerRegistry.Add(Player.Id, Role.Id);
        }
    }
}
