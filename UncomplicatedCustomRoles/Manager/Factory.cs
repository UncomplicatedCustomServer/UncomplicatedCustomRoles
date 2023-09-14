using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace UncomplicatedCustomRoles.Manager
{
    public class Factory
    {
        public static List<Room> RoomsInZone(ZoneType Zone)
        {
            List<Room> Rooms = new();
            foreach (Room Room in Room.List)
            {
                if (Room.Zone == Zone)
                {
                    Rooms.Add(Room);
                }
            }
            return Rooms;
        }

        public static List<Room> GetRoomList()
        {
            List<Room> Rooms = new();
            foreach (Room Room in Room.List)
            {
                Rooms.Add(Room);
            }
            return Rooms;
        }
    }
}
