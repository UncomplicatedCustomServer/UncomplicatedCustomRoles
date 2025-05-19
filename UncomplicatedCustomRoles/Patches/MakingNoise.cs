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
