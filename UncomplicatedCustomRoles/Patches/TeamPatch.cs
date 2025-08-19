/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

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
        static bool Prefix(Player __instance, ref Team __result) => !DisguiseTeam.List.TryGetValue(__instance.PlayerId, out __result);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.IsSCP), MethodType.Getter)]
    internal class IsScpPatch
    {
        static bool Prefix(Player __instance, ref bool __result)
        {
            if (DisguiseTeam.List.TryGetValue(__instance.PlayerId, out Team team)) {
                __result = team is Team.SCPs;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.IsChaos), MethodType.Getter)]
    internal class IsChaosPatch
    {
        static bool Prefix(Player __instance, ref bool __result)
        {
            if (DisguiseTeam.List.TryGetValue(__instance.PlayerId, out Team team))
            {
                __result = team is Team.ChaosInsurgency;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.IsNTF), MethodType.Getter)]
    internal class IsNtfPatch
    {
        static bool Prefix(Player __instance, ref bool __result)
        {
            if (DisguiseTeam.List.TryGetValue(__instance.PlayerId, out Team team))
            {
                __result = team is not Team.FoundationForces;
                return false;
            }

            return true;
        }
    }

    // Achievements
    /*[HarmonyPatch(typeof(GeneralKillsHandler), nameof(GeneralKillsHandler.HandleAttackerKill))]
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

            if (index != -1)
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
            

            if (index != 1)
            {
                newInstructions[index] = new(OpCodes.Ldfld, Field(typeof(Footprint), nameof(Footprint.Hub)));
                newInstructions[index + 1] = new(OpCodes.Call, Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new Type[] { typeof(ReferenceHub) }));
            }

            return newInstructions;
        }
    }*/

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
        private static bool Prefix(HumanRole ply, DamageHandlerBase dh, ref bool __result)
        {
            if (dh is AttackerDamageHandler attackerDamageHandler)
            {
                __result = attackerDamageHandler.Attacker.Role.GetTeam() == ply.Team;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new Type[] {typeof(ReferenceHub) })]
    internal class PlayerRolesUtilsTeamPatch
    {
        private static bool Prefix(ReferenceHub hub, ref Team __result)
        {
            if (DisguiseTeam.List.TryGetValue(hub.PlayerId, out __result))
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetFaction), new Type[] { typeof(ReferenceHub) })]
    internal class PlayerRolesUtilsFactionPatch
    {
        private static bool Prefix(ReferenceHub hub, ref Faction __result)
        {
            if (DisguiseTeam.List.TryGetValue(hub.PlayerId, out Team team))
            {
                __result = team.GetFaction();
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.IsHuman), new Type[] { typeof(ReferenceHub) })]
    internal class PlayerRolesUtilsIsHumanPatch
    {
        private static bool Prefix(ReferenceHub hub, ref bool __result)
        {
            if (DisguiseTeam.List.TryGetValue(hub.PlayerId, out Team team))
            {
                __result = team != Team.Dead && team != Team.Flamingos && team != 0;
                return false;
            }

            return true;
        }
    }

    /*[HarmonyPatch(typeof(BePoliteBeEfficientHandler), nameof(BePoliteBeEfficientHandler.HandleDeath))] WE DONT CARE ABOUT ACHIEVEMENTS
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
    }*/

    /*[HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.OnAnyPlayerDied))]
    internal class GenericKillPatch
    {
        private static readonly MethodInfo _attackerProperty = PropertyGetter(typeof(AttackerDamageHandler), nameof(AttackerDamageHandler.Attacker));
        private static readonly MethodInfo _checkForScp = Method(typeof(DisguiseTeam), nameof(DisguiseTeam.Handler1));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new(instructions);
            int index = -1;

            for (int i = 0; i < newInstructions.Count; i++)
                if (newInstructions[i].opcode == OpCodes.Callvirt)
                {
                    index = i;
                    break;
                }

            Label end = new();

            if (index != -1)
            {
                newInstructions.Add(new(OpCodes.Nop));
                newInstructions.Last().labels.Add(end);
                newInstructions[index] = new(OpCodes.Callvirt, _attackerProperty);
                newInstructions.Insert(index + 1, new(OpCodes.Call, _checkForScp));
                newInstructions.Insert(index + 2, new(OpCodes.Brtrue, end));
            }

            return newInstructions;
        }
    }*/

    // We directly modify the IsEnemy method in order to handle everything
    [HarmonyPatch(typeof(AttackerDamageHandler), nameof(AttackerDamageHandler.ProcessDamage))]
    internal class AttackerDamagePatch
    {
        private static readonly FieldInfo _footprintRole = Field(typeof(Footprint), nameof(Footprint.Role));

        private static readonly MethodInfo _moddedHitboxIsEnemy = Method(typeof(DisguiseTeam), nameof(DisguiseTeam.IsEnemy));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new(instructions);
            int index = -1;

            for (int i = 0; i < newInstructions.Count; i++)
                if (newInstructions[i].opcode == OpCodes.Call && newInstructions[i].operand is FieldInfo methodInfo && methodInfo == _footprintRole)
                {
                    index = i;
                    break;
                }

            if (index != -1)
            {
                newInstructions.RemoveRange(index, 4);
                newInstructions.InsertRange(index, new CodeInstruction[] {
                    new(OpCodes.Ldarg_1),
                    new(OpCodes.Call, _moddedHitboxIsEnemy)
                });
            }

            return newInstructions;
        }
    }

    // Most important patch
    [HarmonyPatch(typeof(HumanRole), nameof(HumanRole.Team), MethodType.Getter)]
    internal class RoleHumanPatch
    {
        static bool Prefix(HumanRole __instance, ref Team __result)
        {
            if (__instance._lastOwner is null)
                return true;

            if (DisguiseTeam.List.TryGetValue(__instance._lastOwner.PlayerId, out __result))
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(FpcStandardScp), nameof(FpcStandardScp.Team), MethodType.Getter)]
    internal class RoleScpPatch
    {
        static bool Prefix(FpcStandardScp __instance, ref Team __result)
        {
            if (__instance._lastOwner is null)
                return true;

            if (DisguiseTeam.List.TryGetValue(__instance._lastOwner.PlayerId, out __result))
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(Scp079Role), nameof(Scp079Role.Team), MethodType.Getter)]
    internal class RoleScp079Patch
    {
        static bool Prefix(Scp079Role __instance, ref Team __result)
        {
            if (__instance._lastOwner is null)
                return true;

            if (DisguiseTeam.List.TryGetValue(__instance._lastOwner.PlayerId, out __result))
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.CurrentRole), MethodType.Getter)]
    internal class RoleCurrentPatch
    {
        static bool Prefix(PlayerRoleManager __instance, ref PlayerRoleBase __result) => !DisguiseTeam.RoleBaseList.TryGetValue(__instance.Hub.PlayerId, out __result);
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