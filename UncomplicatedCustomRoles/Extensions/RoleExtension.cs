using Footprinting;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using UnityEngine;

namespace UncomplicatedCustomRoles.Extensions
{
    public static class RoleExtension
    {
        public static bool CompareLife(this Footprint footprint, Footprint other) => footprint.LifeIdentifier == other.LifeIdentifier;

        public static bool CompareLife(this Footprint footprint, ReferenceHub other) => footprint.LifeIdentifier == other.roleManager.CurrentRole.UniqueLifeIdentifier;

        public static Color GetColor(this RoleTypeId roleType) => roleType is RoleTypeId.None ? Color.white : roleType.GetRoleBase().RoleColor;

        public static string GetFullName(this RoleTypeId typeId) => typeId.GetRoleBase().RoleName;

        public static PlayerRoleBase GetRoleBase(this RoleTypeId roleType) => roleType.TryGetRoleBase(out PlayerRoleBase roleBase) ? roleBase : null;

        public static bool TryGetRoleBase(this RoleTypeId roleType, out PlayerRoleBase roleBase) => PlayerRoleLoader.TryGetRoleTemplate(roleType, out roleBase);

        public static bool TryGetRoleBase<T>(this RoleTypeId roleType, out T roleBase) where T : PlayerRoleBase => PlayerRoleLoader.TryGetRoleTemplate(roleType, out roleBase);

        public static Vector3 GetRandomSpawnLocation(this RoleTypeId roleType)
        {
            if (roleType.TryGetRoleBase(out FpcStandardRoleBase fpcRole) && fpcRole.SpawnpointHandler != null && fpcRole.SpawnpointHandler.TryGetSpawnpoint(out Vector3 position, out float horizontalRotation))
                return position;

            return Vector3.zero;
        }

    }
}
