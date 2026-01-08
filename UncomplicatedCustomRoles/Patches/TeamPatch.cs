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
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.ThrowableProjectiles;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079.Rewards;
using PlayerRoles.PlayableScps.Scp939.Mimicry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using InventorySystem.Disarming;
using InventorySystem.Items;
using InventorySystem.Searching;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Manager;
using static HarmonyLib.AccessTools;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.CurrentRole), MethodType.Getter)]
    internal class PlayerRoleManagerPatch
    {
        private static readonly HashSet<string> _blockedMethods = new()
        {
            $"{typeof(DoorPermissionsPolicy)}::{nameof(DoorPermissionsPolicy.CheckPermissions)}",
            $"{typeof(DoorPermissionsPolicyExtensions)}::{nameof(DoorPermissionsPolicyExtensions.GetCombinedPermissions)}"
        };

        static bool Prefix(PlayerRoleManager __instance, ref PlayerRoleBase __result)
        {
            if (__instance.Hub?.netId is 0)
                return true;

            if (__instance.Hub is not null && DisguiseTeam.RoleBaseList.TryGetValue(__instance.Hub.PlayerId, out PlayerRoleBase role))
            {
                if (role is null)
                    LogManager.Error($"[UCR] Disguised role for player {__instance.Hub.PlayerId} is null!");

                StackTrace trace = new();

                for (int i = 0; i < trace.FrameCount; i++)
                {
                    StackFrame frame = trace.GetFrame(i);

                    if (_blockedMethods.Contains($"{frame.GetMethod().DeclaringType.FullName}::{frame.GetMethod().Name}"))
                        return true;
                }

                __result = role;

                return false;
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetRoleId))]
    internal class PlayerRolesUtilsPatch
    {
        private static readonly Dictionary<Team, RoleTypeId> _roleTeam = new()
        {
            { Team.ClassD, RoleTypeId.ClassD },
            { Team.SCPs, RoleTypeId.Scp0492 },
            { Team.Scientists, RoleTypeId.Scientist },
            { Team.ChaosInsurgency, RoleTypeId.ChaosConscript },
            { Team.FoundationForces, RoleTypeId.NtfPrivate },
            { Team.Flamingos, RoleTypeId.Flamingo },
            { Team.OtherAlive, RoleTypeId.Tutorial }
        };

        private static readonly HashSet<string> allowedMethods = new()
        {
            $"{typeof(HitboxIdentity)}::{nameof(HitboxIdentity.IsEnemy)}",
            $"{typeof(GeneralKillsHandler)}::{nameof(GeneralKillsHandler.HandleAttackerKill)}",
            $"{typeof(TerminationRewards)}::{nameof(TerminationRewards.EvaluateGainReason)}",
            $"{typeof(MimicryRecorder)}::{nameof(MimicryRecorder.WasKilledByTeammate)}",
            $"{typeof(ExplosionGrenade)}::{nameof(ExplosionGrenade.Explode)}"
        };

        static bool Prefix(ReferenceHub hub, ref RoleTypeId __result)
        {
            if (!DisguiseTeam.List.TryGetValue(hub.PlayerId, out Team team))
                return true;

            StackTrace trace = new();

            for (int i = 0; i < trace.FrameCount; i++)
            {
                StackFrame frame = trace.GetFrame(i);

                if (allowedMethods.Contains($"{frame.GetMethod().DeclaringType.FullName}::{frame.GetMethod().Name}"))
                {
                    //LogManager.Info($"[{i}] - {frame.GetMethod().DeclaringType.FullName}::{frame.GetMethod().Name} - {frame.GetFileName()} - {frame.GetFileLineNumber()}");
                    __result = _roleTeam[team];
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ExplosionGrenade), nameof(ExplosionGrenade.ExplodeDestructible))]
    internal class GrenadeTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new(instructions);
            int index = -1;

            for (int i = 0; i < newInstructions.Count; i++)
            {
                if (newInstructions[i].opcode == OpCodes.Call && newInstructions[i].operand is MethodInfo method && method == Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetRoleId), new Type[] { typeof(ReferenceHub) }))
                {
                    index = i;
                    break;
                }
            }

            newInstructions[index+1].operand = Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), new Type[] { typeof(ReferenceHub) });
            newInstructions.RemoveAt(index);

            return newInstructions;
        }
    }

    [HarmonyPatch(typeof(PickupSearchCompletor), nameof(PickupSearchCompletor.ValidateAny))]
    public class PickupSearchCompletorPatch
    {
        static bool Prefix(PickupSearchCompletor __instance, ref bool __result)
        {
            if (!DisguiseTeam.List.TryGetValue(__instance.Hub.PlayerId, out Team team) || team != Team.SCPs ||
                __instance.Hub.roleManager.CurrentRole.RoleTypeId.GetTeam() == Team.SCPs) return true;
            __result = !__instance.TargetPickup.Info.Locked && !__instance.Hub.inventory.IsDisarmed() &&
                       !__instance.Hub.interCoordinator.AnyBlocker(BlockedInteraction.GrabItems);
            return false;
        }
    }
}