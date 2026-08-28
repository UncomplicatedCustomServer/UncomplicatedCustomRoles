/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Footprinting;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using Respawning.NamingRules;
using UnityEngine;

namespace UncomplicatedCustomRoles.Extensions;

public static class RoleExtension
{
    public static bool CompareLife(this Footprint footprint, Footprint other)
    {
        return footprint.LifeIdentifier == other.LifeIdentifier;
    }

    public static bool CompareLife(this Footprint footprint, ReferenceHub other)
    {
        return footprint.LifeIdentifier == other.roleManager.CurrentRole.UniqueLifeIdentifier;
    }

    public static Color GetColor(this RoleTypeId roleType)
    {
        return roleType is RoleTypeId.None ? Color.white : roleType.GetRoleBase()?.RoleColor ?? Color.white;
    }

    public static string GetFullName(this RoleTypeId typeId)
    {
        return typeId.GetRoleBase()?.RoleName ?? string.Empty;
    }

    public static PlayerRoleBase GetRoleBase(this RoleTypeId roleType)
    {
        return roleType.TryGetRoleBase(out var roleBase) ? roleBase : null;
    }

    public static bool TryGetRoleBase(this RoleTypeId roleType, out PlayerRoleBase roleBase)
    {
        return roleType.TryGetRoleTemplate(out roleBase);
    }

    public static bool TryGetRoleBase<T>(this RoleTypeId roleType, out T roleBase) where T : PlayerRoleBase
    {
        return roleType.TryGetRoleTemplate(out roleBase);
    }

    public static bool TryGetLatestUnitNameId(this Team team, out byte unitNameId)
    {
        unitNameId = 0;

        if (!NamingRulesManager.TryGetNamingRule(team, out _) ||
            !NamingRulesManager.GeneratedNames.TryGetValue(team, out var names) || names.Count is 0)
            return false;

        unitNameId = (byte)Mathf.Min(names.Count - 1, byte.MaxValue);
        return true;
    }

    public static Vector3 GetRandomSpawnLocation(this RoleTypeId roleType)
    {
        if (roleType.TryGetRoleBase(out FpcStandardRoleBase fpcRole) && fpcRole.SpawnpointHandler != null &&
            fpcRole.SpawnpointHandler.TryGetSpawnpoint(out var position, out var horizontalRotation))
            return position;

        return Vector3.zero;
    }
}