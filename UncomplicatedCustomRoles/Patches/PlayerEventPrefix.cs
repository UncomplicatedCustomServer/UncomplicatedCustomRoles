/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.Events.EventArgs.Interfaces;
using Exiled.Events.Features;
using HarmonyLib;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(Event<object>), nameof(Event<object>.InvokeSafely))]
    internal class PlayerEventPrefix
    {
        public static void Prefix(object arg)
        {
            if (arg is IDeniableEvent deniable && !deniable.IsAllowed)
                return;

            if (arg is IPlayerEvent ev && ev.Player is not null && ev.Player.TryGetSummonedInstance(out SummonedCustomRole customRole))
            {
                // Invoke for EventCustomRole(s)
                CustomRoleEventHandler.InvokeAllSafely(ev);

                string name = arg.GetType().Name.Replace("EventArgs", string.Empty);

                foreach (CustomModule module in customRole.CustomModules)
                    if (module.TriggerOnEvents.Contains(name))
                        if (!module.OnEvent(name, ev) && arg is IDeniableEvent deniableEvent)
                            deniableEvent.IsAllowed = false;
            }
        }
    }
}
