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
using PlayerRoles;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Integrations
{
    internal static class LabApiExtensions
    {
        private const string PluginName = "LabApiExtensions";

        public static bool IsAvailable =>
            DynamicInvoke.GetMethod(PluginName, "LabApiExtensions.Managers.FakeRoleManager.AddFakeRole", false, 2) != null;

        public static void AddFakeRole(Player player, RoleTypeId roleType)
        {
            try
            {
                DynamicInvoke.GetMethod(PluginName, "LabApiExtensions.Managers.FakeRoleManager.AddFakeRole", false, 2)
                    ?.Invoke(null, new object[] { player, roleType });
            }
            catch (Exception e)
            {
                LogManager.Error($"[LabApiExtensions] Failed to AddFakeRole for {player?.Nickname}: {e}");
            }
        }

        public static void RemoveFakeRole(Player player)
        {
            try
            {
                DynamicInvoke.GetMethod(PluginName, "LabApiExtensions.Managers.FakeRoleManager.RemoveFakeRole", false, 1)
                    ?.Invoke(null, new object[] { player });
            }
            catch (Exception e)
            {
                LogManager.Error($"[LabApiExtensions] Failed to RemoveFakeRole for {player?.Nickname}: {e}");
            }
        }
    }
}
