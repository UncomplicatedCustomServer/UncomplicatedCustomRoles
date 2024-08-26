using PlayerRoles.FirstPersonControl;
using PlayerRoles;
using UnityEngine;

namespace UncomplicatedCustomRoles.Extensions
{
    internal static class RoleTypeExtension
    {
        public static Vector3 GetRandomSpawnLocation(this RoleTypeId roleType)
        {
            if (roleType.TryGetRoleBase(out PlayerRoleBase roleBaseFpc) && roleBaseFpc is FpcStandardRoleBase roleBase && roleBase.SpawnpointHandler != null && roleBase.SpawnpointHandler.TryGetSpawnpoint(out Vector3 position, out _))
            {
                return position;
            }

            return new(0, 0, 0);
        }

        public static bool TryGetRoleBase(this RoleTypeId roleType, out PlayerRoleBase roleBase)
        {
            return PlayerRoleLoader.TryGetRoleTemplate(roleType, out roleBase);
        }
    }
}
