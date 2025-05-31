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
using PlayerRoles.FirstPersonControl.Thirdperson;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(AnimatedCharacterModel), nameof(AnimatedCharacterModel.PlayFootstep))]
    internal class MakingNoise
    {
        static bool Prefix(AnimatedCharacterModel __instance)
        {
            if (__instance.OwnerHub.TryGetSummonedInstance(out SummonedCustomRole summonedInstance) && summonedInstance.HasModule<SilentWalker>())
                return false;

            return true;
        }
    }
}
