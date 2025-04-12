/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.API.Features;
using HarmonyLib;
using System;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(InventorySystem.Configs.InventoryLimits), nameof(InventorySystem.Configs.InventoryLimits.GetCategoryLimit), new Type[] { typeof(ItemCategory), typeof(ReferenceHub)})]
    internal class InventoryLimitsPrefix
    {
        static bool Prefix(ItemCategory category, ReferenceHub player, ref sbyte __result)
        {
            if (player is null)
                return true;

            if (Player.TryGet(player, out Player realPlayer) && realPlayer.TryGetSummonedInstance(out SummonedCustomRole summonedInstance) && summonedInstance.Role.CustomInventoryLimits is not null && summonedInstance.Role.CustomInventoryLimits.ContainsKey(category))
            {
                __result = summonedInstance.Role.CustomInventoryLimits[category];
                return false;
            }

            return true;
        }
    }
}
