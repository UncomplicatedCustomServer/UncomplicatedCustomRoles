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

namespace UncomplicatedCustomRoles.Integrations
{
    internal static class SLWardobe
    {
        public static object PluginInstance { get; } = DynamicInvoke.GetMethod("SLWardrobe", "SLWardrobe.SLWardrobe.Instance_get");

        public static void ApplySuit(Player player, string suitName)
        {
            if (PluginInstance is null)
            {
                LogManager.Error($"Failed to run SLWardrobe.ApplySuit(): Instance of the plugin not found!");
                return;
            }

            MethodInfo method = DynamicInvoke.GetMethod("SLWardrobe", "SLWardrobe.SLWardrobe.ApplySuit");

            if (method is null)
            {
                LogManager.Error($"Failed to run SLWardrobe.ApplySuit(): Method not found!");
                return;
            }

            LogManager.Silent($"ArgsCounter_ {method.GetParameters().Length} for 2 - expected: {string.Join(", ", method.GetParameters().Select(p => p.ParameterType.FullName))} - found: {player?.GetType().FullName}, {suitName.GetType().FullName}");
            DynamicInvoke.GetMethod("SLWardrobe", "SLWardrobe.SLWardrobe.ApplySuit")?.Invoke(PluginInstance, new object[] { player, suitName });
        }

        public static void RemoveSuit(Player player)
        {
            DynamicInvoke.GetMethod("SLWardrobe", "SLWardrobe.SuitBinder.RemoveSuit")?.Invoke(null, new object[] { player });
        }
    }
}