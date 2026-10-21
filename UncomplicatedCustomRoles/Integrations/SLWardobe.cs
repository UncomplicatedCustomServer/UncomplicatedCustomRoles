/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Linq;
using System.Reflection;
using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Integrations;

internal static class SLWardobe
{
    public static object PluginInstance { get; } = DynamicInvoke.GetMethod("SLWardrobe", "SLWardrobe.SLWardrobe.Instance_get")?.Invoke(null, null);

    public static void ApplySuit(Player player, string suitName)
    {
        if (PluginInstance is null)
        {
            LogManager.Error("Failed to run SLWardrobe.ApplySuit(): Instance of the plugin not found!");
            return;
        }

        MethodInfo method = DynamicInvoke.GetMethod("SLWardrobe", "SLWardrobe.SLWardrobe.ApplySuit");

        if (method is null)
        {
            LogManager.Error("Failed to run SLWardrobe.ApplySuit(): Method not found!");
            return;
        }

        MethodInfo playerMethod = DynamicInvoke.GetMethod("Exiled.API", "Exiled.API.Features.Player.Get", false, 1, ["apiPlayer"]);

        object playerObject = playerMethod?.Invoke(null, [player]) ?? player;

        LogManager.Silent($"ArgsCounter_ {method.GetParameters().Length} for 2 - expected: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.FullName))} - found: {playerObject?.GetType().FullName}, {suitName.GetType().FullName}");
        method.Invoke(PluginInstance, [playerObject, suitName]);
    }

    public static void RemoveSuit(Player player)
    {
        if (PluginInstance is null)
        {
            LogManager.Error("Failed to run SLWardrobe.RemoveSuit(): Instance of the plugin not found!");
            return;
        }

        MethodInfo method = DynamicInvoke.GetMethod("SLWardrobe", "SLWardrobe.SuitBinder.RemoveSuit");

        if (method is null)
        {
            LogManager.Error("Failed to run SLWardrobe.RemoveSuit(): Method not found!");
            return;
        }

        MethodInfo playerMethod = DynamicInvoke.GetMethod("Exiled.API", "Exiled.API.Features.Player.Get", false, 1, ["apiPlayer"]);
        object playerObject = playerMethod.Invoke(null, [player]) ?? player;
        
        LogManager.Silent($"ArgsCounter_ {method.GetParameters().Length} for 1 - expected: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.FullName))} - found: {playerObject?.GetType().FullName}");
        method.Invoke(PluginInstance, [playerObject]);
    }
}