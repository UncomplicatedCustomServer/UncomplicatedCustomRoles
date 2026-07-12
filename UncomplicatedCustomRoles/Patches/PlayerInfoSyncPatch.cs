/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using HarmonyLib;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Patches;

[HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.Network_customPlayerInfoString), MethodType.Setter)]
internal class CustomPlayerInfoSyncPatch
{
    private static bool Prefix(NicknameSync __instance, string value)
    {
        if (CustomInfo.SuppressExternalSync)
            return true;

        if (__instance._hub is not null && __instance._hub.TryGetSummonedInstance(out var role) &&
            role.CustomInfo is not null)
        {
            if (role.CustomInfo.Info != value)
                role.CustomInfo.Info = value;

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.Network_playerInfoToShow), MethodType.Setter)]
internal class PlayerInfoAreaSyncPatch
{
    private static void Prefix(NicknameSync __instance, ref PlayerInfoArea value)
    {
        if (CustomInfo.SuppressExternalSync)
            return;

        if (__instance._hub is not null && __instance._hub.TryGetSummonedInstance(out var _))
        {
            value |= PlayerInfoArea.CustomInfo;
            value &= ~PlayerInfoArea.Role;
            value &= ~PlayerInfoArea.Nickname;
            value &= ~PlayerInfoArea.UnitName;
        }
    }
}