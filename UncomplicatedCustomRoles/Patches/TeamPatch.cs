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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using InventorySystem.Disarming;
using InventorySystem.Items;
using InventorySystem.Searching;
using LabApi.Features.Console;
using MapGeneration.Distributors;
using PlayerRoles.PlayableScps.Scp079;
using PlayerStatsSystem;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Manager;
using static HarmonyLib.AccessTools;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.CurrentRole), MethodType.Getter)]
    internal class PlayerRoleManagerPatch
    {
        static bool Prefix(PlayerRoleManager __instance, ref PlayerRoleBase __result)
        {
            if (__instance.Hub == null || __instance.Hub.netId == 0)
                return true;

            if (__instance.Hub is not null && DisguiseTeam.RoleBaseList.TryGetValue(__instance.Hub.PlayerId, out PlayerRoleBase role))
            {
                if (role is null)
                    LogManager.Error($"Disguised role for player {__instance.Hub.PlayerId} is null!");

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
            $"{typeof(ExplosionGrenade)}::{nameof(ExplosionGrenade.Explode)}",
            $"{typeof(FlashbangGrenade)}::{nameof(FlashbangGrenade.ServerFuseEnd)}",
            $"{typeof(AttackerDamageHandler)}::{nameof(AttackerDamageHandler.ProcessDamage)}",
            $"{typeof(Scp079Recontainer)}::{nameof(Scp079Recontainer.OnServerRoleChanged)}"
        };

        static bool Prefix(ReferenceHub hub, ref RoleTypeId __result)
        {
            if (hub == null)
                return true;
            
            if (!DisguiseTeam.List.TryGetValue(hub.PlayerId, out Team team))
                return true;

            StackTrace trace = new();

            for (int i = 0; i < trace.FrameCount; i++)
            {
                StackFrame frame = trace.GetFrame(i);
                /*if (frame.GetMethod().Name.Contains("FpcServerPositionDistributor") || frame.GetMethod().Name.Contains("GetVisibleRole") ||
                    frame.GetMethod().DeclaringType.FullName.Contains("RemoteAdmin"))
                    break;

                if (frame.GetMethod().DeclaringType.FullName.Contains("UncomplicatedCustomRoles") || frame.GetMethod().Name.Contains("GetRoleId_Patch1"))
                {}
                else
                    LogManager.Debug($"[{i}] - {frame.GetMethod().DeclaringType.FullName}::{frame.GetMethod().Name} - {frame.GetFileName()} - {frame.GetFileLineNumber()}");
                */
                if (allowedMethods.Contains($"{frame.GetMethod().DeclaringType.FullName}::{frame.GetMethod().Name}"))
                {
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
    
    [HarmonyPatch]
    public class DoorPermissionsPolicyPatch
    {
        static MethodBase TargetMethod()
        {
            return Method(typeof(DoorPermissionsPolicy), "CheckPermissions", new[] { typeof(ReferenceHub), typeof(IDoorPermissionRequester), typeof(PermissionUsed).MakeByRefType() });
        }

        static bool Prefix(DoorPermissionsPolicy __instance, ReferenceHub hub, IDoorPermissionRequester requester, out PermissionUsed callback, ref bool __result)
        {
            callback = null;
            if (__instance.RequiredPermissions == DoorPermissionFlags.None || hub.serverRoles.BypassMode)
            {
                __result = true;
                return false;
            }
            if (hub.roleManager.CurrentRole is IDoorPermissionProvider currentRole &&
                (!DisguiseTeam.List.TryGetValue(hub.PlayerId, out Team team) || team != Team.SCPs))
            {
                __result = __instance.CheckPermissions(currentRole, requester, out callback);
                return false;
            }
            ItemBase curInstance = hub.inventory.CurInstance;
            __result = curInstance != null && curInstance is IDoorPermissionProvider provider && __instance.CheckPermissions(provider, requester, out callback);
            return false;
        }
    }
    
    [HarmonyPatch(typeof(DoorPermissionsPolicyExtensions), nameof(DoorPermissionsPolicyExtensions.GetCombinedPermissions))]
    public class DoorPermissionsPolicyExtensionsPatch
    {
        static bool Prefix(ReferenceHub hub, IDoorPermissionRequester requester, ref DoorPermissionFlags __result)
        {
            if (hub == null)
            {
                __result = DoorPermissionFlags.None;
                return false;
            }

            if (hub.serverRoles.BypassMode)
            {
                __result = DoorPermissionFlags.All;
                return false;
            }

            DoorPermissionFlags combinedPermissions = DoorPermissionFlags.None;

            if (hub.roleManager.CurrentRole is IDoorPermissionProvider currentRole &&
                (!DisguiseTeam.List.TryGetValue(hub.PlayerId, out Team team) || team != Team.SCPs))
                combinedPermissions |= currentRole.GetPermissions(requester);

            ItemBase curInstance = hub.inventory.CurInstance;
            if (curInstance != null && curInstance is IDoorPermissionProvider permissionProvider)
                combinedPermissions |= permissionProvider.GetPermissions(requester);

            __result = combinedPermissions;
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.OnServerRoleChanged))]
    public class Scp079RecontainerPatch
    {
        static bool Prefix(Scp079Recontainer __instance, ReferenceHub hub, RoleTypeId newRole, RoleChangeReason reason)
        {
            Team team = hub.GetRoleId().GetTeam();
            if (DisguiseTeam.List.TryGetValue(hub.PlayerId, out Team t))
                team = t;
            LogManager.Debug($"Player {hub.PlayerId} changed role to {newRole} for reason {reason}. Checking if recontainment is needed...");
            LogManager.Debug($"Player's current role: {hub.GetRoleId()}, team: {team}");
            if (newRole != RoleTypeId.Spectator || !IsScpButNot079(hub.GetRoleId(), team) || Scp079Role.ActiveInstances.Count == 0 ||
                ReferenceHub.AllHubs.Any(x =>
                {
                    if (x == hub)
                        return false;

                    Team effectiveTeam = x.GetRoleId().GetTeam();
                    if (DisguiseTeam.List.TryGetValue(x.PlayerId, out Team fakeTeam))
                        effectiveTeam = fakeTeam;

                    return IsScpButNot079(x.GetRoleId(), effectiveTeam);
                }))
                return false;
            __instance.SetContainmentDoors(true, true);
            __instance.Recontain(true);
            foreach (Scp079Generator allGenerator in Scp079Recontainer.AllGenerators)
                allGenerator.Engaged = true;
            return false;
        }

        private static bool IsScpButNot079(RoleTypeId roleTypeId, Team team)
        {
            LogManager.Debug($"Checking if role {roleTypeId} is an SCP but not 079 for team {team}");
            return team == Team.SCPs && roleTypeId != RoleTypeId.Scp079;
        }
    }
}