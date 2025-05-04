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

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    internal class ServerNamePatch
    {
        private static void Postfix() => ServerConsole._serverName += $"<color=#00000000><size=1>UCR {Plugin.Instance.Version.ToString(3)}</size></color>";
    }
}
