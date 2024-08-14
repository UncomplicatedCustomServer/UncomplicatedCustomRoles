using Zone = PluginAPI.Core.Zones.FacilityZone;
using MapGeneration;
using PluginAPI.Core.Zones;
using PluginAPI.Core;
using UnityEngine;
using PlayerRoles.PlayableScps.Scp079;
using System.Linq;

namespace UncomplicatedCustomRoles.API.Features
{
    public class Room : FacilityRoom
    {
        /// <summary>
        /// Initialize a new <see cref="FacilityRoom"/> instance
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="identifier"></param>
        public Room(Zone zone, RoomIdentifier identifier) : base(zone, identifier) { }

        // Thanks to EXILED
        /// <summary>
        /// Gets a <see cref="Room"/> by it's <see cref="Vector3"/> position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Room Get(Vector3 position) => RoomIdUtils.RoomAtPositionRaycasts(position, false) is RoomIdentifier identifier ? Get(identifier) : null;

        /// <summary>
        /// Gets a <see cref="Room"/> by it's <see cref="RoomIdentifier"/>
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static Room Get(RoomIdentifier identifier) => Map.Rooms.Where(room => identifier == room).FirstOrDefault()?.ApiRoom as Room;

        // Thanks to EXILED -> https://github.com/ExMod-Team/EXILED/blob/master/EXILED/Exiled.API/Features/Room.cs#L269
        /// <summary>
        /// Tries to find the room that a <see cref="GameObject"/> is inside, first using the <see cref="Transform"/>'s parents, then using a Raycast if no room was found
        /// </summary>
        /// <param name="objectInRoom"></param>
        /// <returns></returns>
        public static Room FindParentRoom(GameObject objectInRoom)
        {
            if (objectInRoom == null)
                return default;

            Room room = null;

            const string playerTag = "Player";

            // First try to find the room owner quickly.
            if (!objectInRoom.CompareTag(playerTag))
            {
                room = objectInRoom.GetComponentInParent<Room>();
            }
            else
            {
                // Check for SCP-079 if it's a player
                Player ply = Player.Get(objectInRoom);

                // Raycasting doesn't make sense,
                // SCP-079 position is constant,
                // let it be 'Outside' instead
                if (ply.RoleBase is Scp079Role scp079Role)
                    room = FindParentRoom(scp079Role.CurrentCamera.gameObject);
            }

            // Finally, try for objects that aren't children, like players and pickups.
            return room ?? Get(objectInRoom.transform.position) ?? default;
        }
    }
}
