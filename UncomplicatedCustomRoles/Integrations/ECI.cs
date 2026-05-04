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
using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Integrations;

internal static class ECI
{
    internal static object PluginInstance { get; } = DynamicInvoke.GetMethod("Exiled.CustomItems", "Exiled.CustomItems.CustomItems.Instance_get")?.Invoke(null, null);

    public static void GiveCustomItem(uint id, Player player)
    {
        try
        {
            if (PluginInstance is null)
            {
                LogManager.Error($"Failed to run Exiled.CustomItems.GiveCustomItem({id}): Instance of the plugin not found!");
                return;
            }
            var tryGiveMethod = DynamicInvoke.GetMethod("Exiled.CustomItems", "Exiled.CustomItems.API.Features.CustomItem.TryGive",
                false, 3, new[] { "id" });

            if (tryGiveMethod is null)
            {
                LogManager.Error(
                    $"Failed to run Exiled.CustomItems.TryGiveCustomItem({id}): CustomItems.TryGive method not found!");
                return;
            }

            var exiledPlayerMethod = DynamicInvoke.GetMethod("Exiled.API", "Exiled.API.Features.Player.Get", false, 1,
                new[] { "apiPlayer" });
            if (exiledPlayerMethod is null)
            {
                LogManager.Error(
                    $"Failed to run Exiled.CustomItems.TryGiveCustomItem({id}): Exiled Player.Get method not found!");
                return;
            }

            var exiledPlayer = exiledPlayerMethod.Invoke(null, new object[] { player });

            var result = tryGiveMethod.Invoke(PluginInstance, new[] { exiledPlayer, id, true });


            if (result is true)
                LogManager.Silent($"Gave custom item id {id} to player {player?.Nickname} via CustomItems.TryGive.");
            else
                LogManager.Warn($"Failed to give custom item id {id} to player {player?.Nickname}. Check if the CustomItem exists");
        }
        catch (Exception e)
        {
            LogManager.Error($"Exception while trying to give CustomItem id {id}: {e}");
        }
    }
}