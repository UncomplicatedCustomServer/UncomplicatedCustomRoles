using Exiled.API.Enums;
using Exiled.API.Extensions;
using MapGeneration;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UncomplicatedCustomRoles.Extensions
{
    internal static class CompatibilityExtension
    {
        public static Dictionary<AmmoType, ushort> GetAmmoTypes(this Dictionary<ItemType, ushort> types)
        {
            Dictionary<AmmoType, ushort> result = new();

            foreach (KeyValuePair<ItemType, ushort> pair in types)
                result.Add(pair.Key.GetAmmoType(), pair.Value);

            return result;
        }

        public static ZoneType GetZoneType(this FacilityZone zone) => zone switch
        {
            FacilityZone.None => ZoneType.Unspecified,
            FacilityZone.Other => ZoneType.Other,
            FacilityZone.LightContainment => ZoneType.LightContainment,
            FacilityZone.HeavyContainment => ZoneType.HeavyContainment,
            FacilityZone.Entrance => ZoneType.Entrance,
            FacilityZone.Surface => ZoneType.Surface,
            // Il caso Pocket non può essere distinto da Other
            _ => ZoneType.Unspecified
        };

        public static List<ZoneType> GetZoneTypes(this IEnumerable<FacilityZone> zones) => zones.Select(z => z.GetZoneType()).ToList();

        private static RoomType FindType(this string room) => room switch
        {
            "PocketWorld" => RoomType.Pocket,
            "Outside" => RoomType.Surface,
            "LCZ_Cafe" => RoomType.LczCafe,
            "LCZ_Toilets" => RoomType.LczToilets,
            "LCZ_TCross" => RoomType.LczTCross,
            "LCZ_Airlock" => RoomType.LczAirlock,
            "LCZ_ChkpA" => RoomType.LczCheckpointA,
            "LCZ_ChkpB" => RoomType.LczCheckpointB,
            "LCZ_Plants" => RoomType.LczPlants,
            "LCZ_Straight" => RoomType.LczStraight,
            "LCZ_Armory" => RoomType.LczArmory,
            "LCZ_Crossing" => RoomType.LczCrossing,
            "LCZ_Curve" => RoomType.LczCurve,
            "LCZ_173" => RoomType.Lcz173,
            "LCZ_330" => RoomType.Lcz330,
            "LCZ_372" => RoomType.LczGlassBox,
            "LCZ_914" => RoomType.Lcz914,
            "LCZ_ClassDSpawn" => RoomType.LczClassDSpawn,
            "HCZ_Nuke" => RoomType.HczNuke,
            "HCZ_TArmory" => RoomType.HczArmory,
            "HCZ_MicroHID_New" => RoomType.HczHid,
            "HCZ_Crossroom_Water" => RoomType.HczCrossRoomWater,
            "HCZ_Testroom" => RoomType.HczTestRoom,
            "HCZ_049" => RoomType.Hcz049,
            "HCZ_079" => RoomType.Hcz079,
            "HCZ_096" => RoomType.Hcz096,
            "HCZ_106_Rework" => RoomType.Hcz106,
            "HCZ_939" => RoomType.Hcz939,
            "HCZ_Tesla_Rework" => RoomType.HczTesla,
            "HCZ_Curve" => RoomType.HczCurve,
            "HCZ_Crossing" => RoomType.HczCrossing,
            "HCZ_Intersection" => RoomType.HczIntersection,
            "HCZ_Intersection_Junk" => RoomType.HczIntersectionJunk,
            "HCZ_Corner_Deep" => RoomType.HczCornerDeep,
            "HCZ_Straight" => RoomType.HczStraight,
            "HCZ_Straight_C" => RoomType.HczStraightC,
            "HCZ_Straight_PipeRoom" => RoomType.HczStraightPipeRoom,
            "HCZ_Straight Variant" => RoomType.HczStraightVariant,
            "HCZ_ChkpA" => RoomType.HczElevatorA,
            "HCZ_ChkpB" => RoomType.HczElevatorB,
            "EZ_GateA" => RoomType.EzGateA,
            "EZ_GateB" => RoomType.EzGateB,
            "EZ_ThreeWay" => RoomType.EzTCross,
            "EZ_Crossing" => RoomType.EzCrossing,
            "EZ_Curve" => RoomType.EzCurve,
            "EZ_PCs" => RoomType.EzPcs,
            "EZ_upstairs" => RoomType.EzUpstairsPcs,
            "EZ_Intercom" => RoomType.EzIntercom,
            "EZ_Smallrooms2" => RoomType.EzSmallrooms,
            "EZ_PCs_small" => RoomType.EzDownstairsPcs,
            "EZ_Chef" => RoomType.EzChef,
            "EZ_Endoof" => RoomType.EzVent,
            "EZ_CollapsedTunnel" => RoomType.EzCollapsedTunnel,
            "EZ_Smallrooms1" => RoomType.EzConference,
            "EZ_Straight" => RoomType.EzStraight,
            "EZ_StraightColumn" => RoomType.EzStraightColumn,
            "EZ_Cafeteria" => RoomType.EzCafeteria,
            "EZ_Shelter" => RoomType.EzShelter,
            "EZ_HCZ_Checkpoint Part" => RoomType.EzCheckpointHallwayA,
            "HCZ_EZ_Checkpoint Part" => RoomType.HczEzCheckpointA,
            _ => RoomType.Unknown,
        };


        public static List<RoomType> GetRoomTypes(this IEnumerable<string> rooms) => rooms.Select(r => r.FindType()).ToList();
    }
}
