/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Reflection;
using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Integrations;

internal static class UCI
{
    public static readonly Assembly Assembly = DynamicInvoke.GetLabAPIAssembly("UncomplicatedCustomItems") ??
                                               DynamicInvoke.GetExiledAssembly("UncomplicatedCustomItems");

    public static readonly Type SummonedCustomItem =
        Assembly?.GetType("UncomplicatedCustomItems.API.Features.SummonedCustomItem");

    public static bool HasCustomItem(uint id, out object customItem)
    {
        customItem = null;


        LogManager.Silent($"UCI found, trying check if the item {id} exists...");

        try
        {
            if ((bool?)DynamicInvoke
                    .GetMethod("UncomplicatedCustomItems", "UncomplicatedCustomItems.API.Utilities.IsCustomItem")
                    ?.Invoke(null,
                        [id]) ?? false)
            {
                customItem = DynamicInvoke
                    .GetMethod("UncomplicatedCustomItems", "UncomplicatedCustomItems.API.Utilities.GetCustomItem")
                    ?.Invoke(null,
                        [id]);

                return customItem is not null;
            }

            return false;
        }
        catch (Exception e)
        {
            LogManager.Error(e.ToString());
            return false;
        }
    }

    public static void GiveCustomItem(uint id, Player player)
    {
        LogManager.Silent($"UCI found, trying to give the item {id} to {player}");

        try
        {
            if (HasCustomItem(id, out var customItem) && customItem is not null)
                SummonedCustomItem?.GetConstructor([
                    Assembly.GetType("UncomplicatedCustomItems.API.Interfaces.ICustomItem"), typeof(Player)
                ]).Invoke([customItem, player]);
        }
        catch (Exception e)
        {
            LogManager.Error(e.ToString());
        }
    }
}