using HarmonyLib;
using PluginAPI.Core;
using System;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.HarmonyElements.Prefix
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
