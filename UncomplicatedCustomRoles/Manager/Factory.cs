using Exiled.API.Enums;
using Exiled.API.Features;
using PlayerRoles;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Structures;
using UnityEngine;

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

        public static Dictionary<RoleTypeId, List<ICustomRole>> RoleIstance()
        {
            return new Dictionary<RoleTypeId, List<ICustomRole>>()
            {
                { RoleTypeId.ClassD, new List<ICustomRole>() },
                { RoleTypeId.Scientist, new List<ICustomRole>() },
                { RoleTypeId.NtfPrivate, new List<ICustomRole>() },
                { RoleTypeId.NtfSergeant, new List<ICustomRole>() },
                { RoleTypeId.NtfCaptain, new List<ICustomRole>() },
                { RoleTypeId.NtfSpecialist, new List<ICustomRole>() },
                { RoleTypeId.ChaosConscript, new List<ICustomRole>() },
                { RoleTypeId.ChaosMarauder, new List<ICustomRole>() },
                { RoleTypeId.ChaosRepressor, new List<ICustomRole>() },
                { RoleTypeId.ChaosRifleman, new List<ICustomRole>() },
                { RoleTypeId.Tutorial, new List<ICustomRole>() },
                { RoleTypeId.Scp049, new List<ICustomRole>() },
                { RoleTypeId.Scp0492, new List<ICustomRole>() },
                { RoleTypeId.Scp079, new List<ICustomRole>() },
                { RoleTypeId.Scp173, new List<ICustomRole>() },
                { RoleTypeId.Scp939, new List<ICustomRole>() },
                { RoleTypeId.Scp096, new List<ICustomRole>() },
                { RoleTypeId.Scp106, new List<ICustomRole>() },
                { RoleTypeId.FacilityGuard, new List<ICustomRole>() }
            };
        }
        public static Vector3 AdjustRoomPosition(Room Room)
        {
            Vector3 Position = Room.Position;
            Position.y += 1.5f;
            return Position;
        }
    }
}
