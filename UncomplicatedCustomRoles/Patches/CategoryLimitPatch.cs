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
using InventorySystem.Configs;
using InventorySystem.Items.Armor;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Patches;

[HarmonyPatch(typeof(InventoryLimits), nameof(InventoryLimits.GetCategoryLimit), typeof(ItemCategory), typeof(ReferenceHub))]
internal static class CategoryLimitByHubPatch
{
    private static void Postfix(ItemCategory category, ReferenceHub player, ref sbyte __result)
    {
        if (TryGetCustomLimit(player, category, out sbyte limit))
            __result = limit;
    }

    internal static bool TryGetCustomLimit(ReferenceHub player, ItemCategory category, out sbyte limit)
    {
        limit = 0;

        if (player is null)
            return false;

        if (player.TryGetSummonedInstance(out SummonedCustomRole role) && role.Role.CustomInventoryLimits is { Count: > 0 } limits && limits.TryGetValue(category, out limit))
            return true;

        return InventoryLimitOverride.TryGet(player.PlayerId, category, out limit);
    }
}

[HarmonyPatch(typeof(InventoryLimits), nameof(InventoryLimits.GetCategoryLimit), typeof(BodyArmor), typeof(ItemCategory))]
internal static class CategoryLimitByArmorPatch
{
    private static void Postfix(BodyArmor armor, ItemCategory category, ref sbyte __result)
    {
        if (armor is not null && CategoryLimitByHubPatch.TryGetCustomLimit(armor.Owner, category, out sbyte limit))
            __result = limit;
    }
}