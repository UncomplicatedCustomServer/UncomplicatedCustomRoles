/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Achievements.Handlers;
using Footprinting;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Rewards;
using PlayerRoles.PlayableScps.Scp939.Mimicry;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UncomplicatedCustomRoles.API.Features;

using static HarmonyLib.AccessTools;

namespace UncomplicatedCustomRoles.Patches
{
#pragma warning disable IDE0051

    [HarmonyPatch(typeof(Player), nameof(Player.Team), MethodType.Getter)]
    internal class TeamPatch
    {
        static bool Prefix(Player __instance, ref Team __result) => !SummonedCustomRole.TryPatchCustomRole(__instance.ReferenceHub, out __result);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.IsSCP), MethodType.Getter)]
    internal class IsScpPatch
    {
        static bool Prefix(Player __instance, ref bool __result) => !SummonedCustomRole.TryCheckForCustomTeam(__instance.ReferenceHub, Team.SCPs, out __result);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.IsChaos), MethodType.Getter)]
    internal class IsChaosPatch
    {
        static bool Prefix(Player __instance, ref bool __result) => !SummonedCustomRole.TryCheckForCustomTeam(__instance.ReferenceHub, Team.ChaosInsurgency, out __result);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.IsNTF), MethodType.Getter)]
    internal class IsNtfPatch
    {
        static bool Prefix(Player __instance, ref bool __result) => !SummonedCustomRole.TryCheckForCustomTeam(__instance.ReferenceHub, Team.FoundationForces, out __result);
    }

    [HarmonyPatch(typeof(GeneralKillsHandler), nameof(GeneralKillsHandler.HandleAttackerKill))]
    internal class HandleAttackerKillPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new(instructions);
            int index = -1;

            for (int i = 0; i < newInstructions.Count; i++)
                if (newInstructions[i].opcode == OpCodes.Ldfld && newInstructions[i].operand is FieldInfo fieldInfo && fieldInfo == Field(typeof(Footprint), nameof(Footprint.Role)))
                {
                    index = i; 
                    break;
                }

            /*if (index != -1)
                newInstructions.InsertRange(index, new CodeInstruction[]
                {
                    // ReferenceHub
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Call, PropertyGetter(typeof(AttackerDamageHandler), nameof(AttackerDamageHandler.Attacker))),
                    new(OpCodes.Ldfld, Field(typeof(Footprint), nameof(Footprint.Hub))),

                    // Actual role
                    new(OpCodes.Ldloc_2),

                    // Get the new role
                    new(OpCodes.Call, Method(typeof(SummonedCustomRole), nameof(SummonedCustomRole.TryGetCusomTeam))),

                    // Save the new role
                    new(OpCodes.Stloc_2),
                });
            */

            if (index != 1)
            {
                newInstructions[index] = new(OpCodes.Ldfld, Field(typeof(Footprint), nameof(Footprint.Hub)));
                newInstructions[index + 1] = new(OpCodes.Call, Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new Type[] { typeof(ReferenceHub) }));
            }

            return newInstructions;
        }
    }

    [HarmonyPatch(typeof(HitboxIdentity), nameof(HitboxIdentity.IsDamageable), new Type[] { typeof(ReferenceHub), typeof(ReferenceHub) })]
    internal class HitboxIdentityDamageablePatch
    {
        static bool Prefix(ReferenceHub attacker, ReferenceHub victim, ref bool __result)
        {
            __result = HitboxIdentity.AllowFriendlyFire || HitboxIdentity.IsEnemy(SummonedCustomRole.TryGetCusomTeam(attacker, attacker.roleManager.CurrentRole.Team), SummonedCustomRole.TryGetCusomTeam(victim, victim.roleManager.CurrentRole.Team));
            return true;
        }
    }

    [HarmonyPatch(typeof(HitboxIdentity), nameof(HitboxIdentity.IsEnemy), new Type[] { typeof(ReferenceHub), typeof(ReferenceHub) })]
    internal class HitboxIdentityEnemyPatch
    {
        static bool Prefix(ReferenceHub attacker, ReferenceHub victim, ref bool __result)
        {
            __result = HitboxIdentity.IsEnemy(SummonedCustomRole.TryGetCusomTeam(attacker, attacker.roleManager.CurrentRole.Team), SummonedCustomRole.TryGetCusomTeam(victim, victim.roleManager.CurrentRole.Team));
            return true;
        }
    }

    [HarmonyPatch(typeof(ExplosionGrenade), nameof(ExplosionGrenade.ExplodeDestructible))]
    internal class ExplodeDestructiblePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new(instructions);
            int index = -1;

            for (int i = 0; i < newInstructions.Count; i++)
                if (newInstructions[i].opcode == OpCodes.Call && newInstructions[i].operand is MethodInfo methodInfo && methodInfo == Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetRoleId), new Type[] { typeof(ReferenceHub )}))
                {
                    index = i;
                    break;
                }

            if (index != -1)
            {
                newInstructions[index] = new(OpCodes.Call, Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new Type[] { typeof(ReferenceHub) }));
                newInstructions.Remove(newInstructions.ElementAt(index + 1));
            }

            return newInstructions;
        }
    }

    [HarmonyPatch(typeof(TerminationRewards), nameof(TerminationRewards.EvaluateGainReason))]
    internal class TerminationRewardPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new(instructions);
            int index = -1;

            for (int i = 0; i < newInstructions.Count; i++)
                if (newInstructions[i].opcode == OpCodes.Ldfld && newInstructions[i].operand is FieldInfo fieldInfo && fieldInfo == Field(typeof(Footprint), nameof(Footprint.Role)))
                {
                    index = i;
                    break;
                }

            if (index != -1)
            {
                newInstructions[index] = new(OpCodes.Ldfld, Field(typeof(Footprint), nameof(Footprint.Hub)));
                newInstructions[index+1] = new(OpCodes.Call, Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new Type[] { typeof(ReferenceHub) }));
            }

            return newInstructions;
        }
    }

    [HarmonyPatch(typeof(MimicryRecorder), nameof(MimicryRecorder.WasKilledByTeammate))]
    internal class MimicryRecorderPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new(instructions);
            int index = -1;

            for (int i = 0; i < newInstructions.Count; i++)
                if (newInstructions[i].opcode == OpCodes.Ldfld && newInstructions[i].operand is FieldInfo fieldInfo && fieldInfo == Field(typeof(Footprint), nameof(Footprint.Role)))
                {
                    index = i;
                    break;
                }

            if (index != -1)
            {
                newInstructions[index] = new(OpCodes.Ldfld, Field(typeof(Footprint), nameof(Footprint.Hub)));
                newInstructions[index + 1] = new(OpCodes.Call, Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new Type[] { typeof(ReferenceHub) }));
            }

            return newInstructions;
        }
    }

    /*[HarmonyPatch(typeof(HumanTerminationTokens), nameof(HumanTerminationTokens.HandleHomocide))]
    internal class HumanTerminationTokensPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new(instructions);
            int index = -1;

            for (int i = 0; i < newInstructions.Count; i++)
                if (newInstructions[i].opcode == OpCodes.Ldfld && newInstructions[i].operand is FieldInfo fieldInfo && fieldInfo == Field(typeof(Footprint), nameof(Footprint.Role)))
                {
                    index = i;
                    break;
                }

            if (index != -1)
            {
                newInstructions[index] = new(OpCodes.Ldfld, Field(typeof(Footprint), nameof(Footprint.Hub)));
                newInstructions[index + 1] = new(OpCodes.Call, Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new Type[] { typeof(ReferenceHub) }));
            }

            return newInstructions;
        }
    }*/

    [HarmonyPatch(typeof(BePoliteBeEfficientHandler), nameof(BePoliteBeEfficientHandler.HandleDeath))]
    internal class BePoliteBeEfficientPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new(instructions);
            int index = -1;

            for (int i = 0; i < newInstructions.Count; i++)
                if (newInstructions[i].opcode == OpCodes.Ldfld && newInstructions[i].operand is FieldInfo fieldInfo && fieldInfo == Field(typeof(Footprint), nameof(Footprint.Role)))
                {
                    index = i;
                    break;
                }

            if (index != -1)
            {
                newInstructions[index] = new(OpCodes.Ldfld, Field(typeof(Footprint), nameof(Footprint.Hub)));
                newInstructions.Insert(index + 1, new(OpCodes.Call, Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new Type[] { typeof(ReferenceHub) })));
                newInstructions[index + 3] = new(OpCodes.Call, Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new Type[] { typeof(ReferenceHub) }));
                newInstructions[index + 4] = new(OpCodes.Call, Method(typeof(HitboxIdentity), nameof(HitboxIdentity.IsEnemy), new Type[] { typeof(Team), typeof(Team) }));
            }

            return newInstructions;
        }
    }

    [HarmonyPatch(typeof(AttackerDamageHandler), nameof(AttackerDamageHandler.ProcessDamage))]
    internal class AttackerDamagePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new(instructions);
            int index = -1;

            for (int i = 0; i < newInstructions.Count; i++)
                if (newInstructions[i].opcode == OpCodes.Ldfld && newInstructions[i].operand is FieldInfo fieldInfo && fieldInfo == Field(typeof(Footprint), nameof(Footprint.Role)))
                {
                    index = i;
                    break;
                }

            if (index != -1)
            {
                newInstructions[index] = new(OpCodes.Ldfld, Field(typeof(Footprint), nameof(Footprint.Hub)));
                newInstructions.Insert(index + 1, new(OpCodes.Call, Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new Type[] { typeof(ReferenceHub) })));
                newInstructions[index + 3] = new(OpCodes.Call, Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new Type[] { typeof(ReferenceHub) }));
                newInstructions[index + 4] = new(OpCodes.Call, Method(typeof(HitboxIdentity), nameof(HitboxIdentity.IsEnemy), new Type[] { typeof(Team), typeof(Team) }));
            }

            return newInstructions;
        }
    }

    // Most important patch
    [HarmonyPatch(typeof(HumanRole), nameof(HumanRole.Team), MethodType.Getter)]
    internal class RoleHumanPatch
    {
        static bool Prefix(HumanRole __instance, ref Team __result) => !SummonedCustomRole.TryPatchCustomRole(TeamPachUtils.WrapReferenceHub(__instance), out __result);
    }

    [HarmonyPatch(typeof(FpcStandardScp), nameof(FpcStandardScp.Team), MethodType.Getter)]
    internal class RoleScpPatch
    {
        static bool Prefix(FpcStandardScp __instance, ref Team __result) => !SummonedCustomRole.TryPatchCustomRole(TeamPachUtils.WrapReferenceHub(__instance), out __result);
    }

    [HarmonyPatch(typeof(Scp079Role), nameof(Scp079Role.Team), MethodType.Getter)]
    internal class RoleScp079Patch
    {
        static bool Prefix(Scp079Role __instance, ref Team __result) => !SummonedCustomRole.TryPatchCustomRole(TeamPachUtils.WrapReferenceHub(__instance), out __result);
    }

    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.CurrentRole), MethodType.Getter)]
    internal class RoleCurrentPatch
    {
        static bool Prefix(PlayerRoleManager __instance, ref PlayerRoleBase __result) => !SummonedCustomRole.TryPatchRoleBase(__instance.Hub, out __result);
    }

    internal class TeamPachUtils
    {
        public static ReferenceHub WrapReferenceHub(PlayerRoleBase instance)
        {
            instance.TryGetOwner(out ReferenceHub hub);
            return hub;
        }
    }
}
