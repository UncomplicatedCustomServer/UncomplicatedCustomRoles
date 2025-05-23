/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using MapGeneration;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Compatibility.PreviousVersionElements.Enums;

namespace UncomplicatedCustomRoles.Extensions
{
    public static class CompatibilityExtension
    {
        public static ItemType GetItemType(this ExiledAmmoType type) => type switch
        {
            ExiledAmmoType.None => ItemType.None,
            ExiledAmmoType.Nato556 => ItemType.Ammo556x45,
            ExiledAmmoType.Nato762 => ItemType.Ammo762x39,
            ExiledAmmoType.Ammo44Cal => ItemType.Ammo44cal,
            ExiledAmmoType.Ammo12Gauge => ItemType.Ammo12gauge,
            ExiledAmmoType.Nato9 => ItemType.Ammo9x19,
            _ => ItemType.None,
        };

        public static List<ItemType> GetItemTypes(this IEnumerable<ExiledAmmoType> types)
        {
            List<ItemType> items = new();

            foreach (ExiledAmmoType ammoType in types)
                items.Add(ammoType.GetItemType());

            return items;
        }

        public static Dictionary<ItemType, T> ConvertItemTypes<T>(this Dictionary<ExiledAmmoType, T> data)
        {
            Dictionary<ItemType, T> items = new();

            foreach (KeyValuePair<ExiledAmmoType, T> item in data)
                items.Add(item.Key.GetItemType(), item.Value);

            return items;
        }

        public static string GetRoomType(this ExiledRoomType type) => type switch
        {
            ExiledRoomType.Pocket => "PocketWorld",
            ExiledRoomType.Surface => "Outside",
            ExiledRoomType.LczCafe => "LCZ_Cafe",
            ExiledRoomType.LczToilets => "LCZ_Toilets",
            ExiledRoomType.LczTCross => "LCZ_TCross",
            ExiledRoomType.LczAirlock => "LCZ_Airlock",
            ExiledRoomType.LczCheckpointA => "LCZ_ChkpA",
            ExiledRoomType.LczCheckpointB => "LCZ_ChkpB",
            ExiledRoomType.LczPlants => "LCZ_Plants",
            ExiledRoomType.LczStraight => "LCZ_Straight",
            ExiledRoomType.LczArmory => "LCZ_Armory",
            ExiledRoomType.LczCrossing => "LCZ_Crossing",
            ExiledRoomType.LczCurve => "LCZ_Curve",
            ExiledRoomType.Lcz173 => "LCZ_173",
            ExiledRoomType.Lcz330 => "LCZ_330",
            ExiledRoomType.LczGlassBox => "LCZ_372",
            ExiledRoomType.Lcz914 => "LCZ_914",
            ExiledRoomType.LczClassDSpawn => "LCZ_ClassDSpawn",
            ExiledRoomType.HczNuke => "HCZ_Nuke",
            ExiledRoomType.HczArmory => "HCZ_TArmory",
            ExiledRoomType.HczHid => "HCZ_MicroHID_New",
            ExiledRoomType.HczCrossRoomWater => "HCZ_Crossroom_Water",
            ExiledRoomType.HczTestRoom => "HCZ_Testroom",
            ExiledRoomType.Hcz049 => "HCZ_049",
            ExiledRoomType.Hcz079 => "HCZ_079",
            ExiledRoomType.Hcz096 => "HCZ_096",
            ExiledRoomType.Hcz106 => "HCZ_106_Rework",
            ExiledRoomType.Hcz939 => "HCZ_939",
            ExiledRoomType.HczTesla => "HCZ_Tesla_Rework",
            ExiledRoomType.HczCurve => "HCZ_Curve",
            ExiledRoomType.HczCrossing => "HCZ_Crossing",
            ExiledRoomType.HczIntersection => "HCZ_Intersection",
            ExiledRoomType.HczIntersectionJunk => "HCZ_Intersection_Junk",
            ExiledRoomType.HczCornerDeep => "HCZ_Corner_Deep",
            ExiledRoomType.HczStraight => "HCZ_Straight",
            ExiledRoomType.HczStraightC => "HCZ_Straight_C",
            ExiledRoomType.HczStraightPipeRoom => "HCZ_Straight_PipeRoom",
            ExiledRoomType.HczStraightVariant => "HCZ_Straight Variant",
            ExiledRoomType.HczElevatorA => "HCZ_ChkpA",
            ExiledRoomType.HczElevatorB => "HCZ_ChkpB",
            ExiledRoomType.EzGateA => "EZ_GateA",
            ExiledRoomType.EzGateB => "EZ_GateB",
            ExiledRoomType.EzTCross => "EZ_ThreeWay",
            ExiledRoomType.EzCrossing => "EZ_Crossing",
            ExiledRoomType.EzCurve => "EZ_Curve",
            ExiledRoomType.EzPcs => "EZ_PCs",
            ExiledRoomType.EzUpstairsPcs => "EZ_upstairs",
            ExiledRoomType.EzIntercom => "EZ_Intercom",
            ExiledRoomType.EzSmallrooms => "EZ_Smallrooms2",
            ExiledRoomType.EzDownstairsPcs => "EZ_PCs_small",
            ExiledRoomType.EzChef => "EZ_Chef",
            ExiledRoomType.EzVent => "EZ_Endoof",
            ExiledRoomType.EzCollapsedTunnel => "EZ_CollapsedTunnel",
            ExiledRoomType.EzConference => "EZ_Smallrooms1",
            ExiledRoomType.EzStraight => "EZ_Straight",
            ExiledRoomType.EzStraightColumn => "EZ_StraightColumn",
            ExiledRoomType.EzCafeteria => "EZ_Cafeteria",
            ExiledRoomType.EzShelter => "EZ_Shelter",
            ExiledRoomType.EzCheckpointHallwayA => "EZ_HCZ_Checkpoint Part",
            ExiledRoomType.EzCheckpointHallwayB => "EZ_HCZ_Checkpoint Part",
            ExiledRoomType.HczEzCheckpointA => "HCZ_EZ_Checkpoint Part",
            ExiledRoomType.HczEzCheckpointB => "HCZ_EZ_Checkpoint Part",
            _ => "Unknown"
        };

        public static List<string> ConvertRoomTypes(this IEnumerable<ExiledRoomType> types)
        {
            List<string> result = new();

            foreach (ExiledRoomType type in types)
                result.Add(type.GetRoomType());

            return result;
        }

        public static FacilityZone GetFacilityZone(this ExiledZoneType zone) => zone switch
        {
            ExiledZoneType.Unspecified => FacilityZone.None,
            ExiledZoneType.Other => FacilityZone.Other,
            ExiledZoneType.LightContainment => FacilityZone.LightContainment,
            ExiledZoneType.HeavyContainment => FacilityZone.HeavyContainment,
            ExiledZoneType.Entrance => FacilityZone.Entrance,
            ExiledZoneType.Surface => FacilityZone.Surface,
            ExiledZoneType.Pocket => FacilityZone.Other,
            _ => FacilityZone.None
        };

        public static List<FacilityZone> ConvertZoneTypes(this IEnumerable<ExiledZoneType> types)
        {
            List<FacilityZone> result = new();

            foreach (ExiledZoneType type in types)
                result.Add(type.GetFacilityZone());

            return result;
        }
    }
}
