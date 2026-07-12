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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Achievements.Handlers;
using Footprinting;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Disarming;
using InventorySystem.Items;
using InventorySystem.Items.ThrowableProjectiles;
using InventorySystem.Searching;
using Mirror;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumanTracker;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Rewards;
using PlayerRoles.PlayableScps.Scp939.Mimicry;
using PlayerStatsSystem;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Manager;
using static HarmonyLib.AccessTools;

namespace UncomplicatedCustomRoles.Patches;

[HarmonyPatchCategory(TeamPatchManager.Category)]
[HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.CurrentRole), MethodType.Getter)]
internal class PlayerRoleManagerPatch
{
    private static bool Prefix(PlayerRoleManager __instance, ref PlayerRoleBase __result)
    {
        var hub = __instance.Hub;
        if (hub is null || !DisguiseTeam.RoleBaseList.TryGetValue(hub.PlayerId, out var role) || role is null)
            return true;

        if (RoleSerializationContext.Active)
            return true;

        __result = role;
        return false;
    }
}

internal static class TeamFakeContext
{
    [ThreadStatic] private static int _depth;

    internal static bool Active => _depth > 0;

    internal static void Enter()
    {
        _depth++;
    }

    internal static void Exit()
    {
        if (_depth > 0)
            _depth--;
    }
}

internal static class RoleSerializationContext
{
    [ThreadStatic] private static int _depth;

    internal static bool Active => _depth > 0;

    internal static void Enter()
    {
        _depth++;
    }

    internal static void Exit()
    {
        if (_depth > 0)
            _depth--;
    }
}

[HarmonyPatchCategory(TeamPatchManager.Category)]
[HarmonyPatch(typeof(RoleSyncInfo), MethodType.Constructor, typeof(ReferenceHub), typeof(RoleTypeId),
    typeof(ReferenceHub), typeof(NetworkWriter))]
internal class RoleSyncInfoCtorPatch
{
    private static void Prefix()
    {
        RoleSerializationContext.Enter();
    }

    private static void Finalizer()
    {
        RoleSerializationContext.Exit();
    }
}

[HarmonyPatchCategory(TeamPatchManager.Category)]
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

    private static bool Prefix(ReferenceHub hub, ref RoleTypeId __result)
    {
        if (hub == null || !TeamFakeContext.Active)
            return true;

        if (!DisguiseTeam.List.TryGetValue(hub.PlayerId, out var team))
            return true;

        if (_roleTeam.TryGetValue(team, out var fakeRole))
        {
            __result = fakeRole;
            return false;
        }

        return true;
    }

    internal static RoleTypeId GetCombatRoleId(ReferenceHub hub)
    {
        if (hub != null && DisguiseTeam.List.TryGetValue(hub.PlayerId, out var team) &&
            _roleTeam.TryGetValue(team, out var fakeRole))
            return fakeRole;

        return hub.GetRoleId();
    }
}

[HarmonyPatchCategory(TeamPatchManager.Category)]
[HarmonyPatch(typeof(AttackerDamageHandler), nameof(AttackerDamageHandler.ProcessDamage))]
internal class ProcessDamageRolePatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new(instructions);
        var original = Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetRoleId), [typeof(ReferenceHub)]);
        var replacement = Method(typeof(PlayerRolesUtilsPatch), nameof(PlayerRolesUtilsPatch.GetCombatRoleId));

        foreach (var instruction in code)
            if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo method && method == original)
                instruction.operand = replacement;

        return code;
    }
}

[HarmonyPatchCategory(TeamPatchManager.Category)]
[HarmonyPatch]
internal class TeamFakeContextPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        return Declared(typeof(HitboxIdentity), nameof(HitboxIdentity.IsEnemy))
            .Concat(Declared(typeof(GeneralKillsHandler), nameof(GeneralKillsHandler.HandleAttackerKill)))
            .Concat(Declared(typeof(TerminationRewards), nameof(TerminationRewards.EvaluateGainReason)))
            .Concat(Declared(typeof(MimicryRecorder), nameof(MimicryRecorder.WasKilledByTeammate)))
            .Concat(Declared(typeof(ExplosionGrenade), nameof(ExplosionGrenade.Explode)))
            .Concat(Declared(typeof(FlashbangGrenade), nameof(FlashbangGrenade.ServerFuseEnd)))
            .Concat(Declared(typeof(AttackerDamageHandler), nameof(AttackerDamageHandler.ProcessDamage)))
            .Concat(Declared(typeof(LastHumanTracker), nameof(LastHumanTracker.IsLastTarget)))
            .Concat(Declared(typeof(Scp079Recontainer), nameof(Scp079Recontainer.OnServerRoleChanged)));
    }

    private static IEnumerable<MethodBase> Declared(Type type, string name)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                               BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => m.Name == name && !m.IsAbstract && !m.ContainsGenericParameters);
    }

    private static void Prefix()
    {
        TeamFakeContext.Enter();
    }

    private static void Finalizer()
    {
        TeamFakeContext.Exit();
    }
}

[HarmonyPatchCategory(TeamPatchManager.Category)]
[HarmonyPatch(typeof(Footprint), MethodType.Constructor, typeof(ReferenceHub))]
internal class FootprintContextPatch
{
    private static void Prefix()
    {
        TeamFakeContext.Enter();
    }

    private static void Finalizer()
    {
        TeamFakeContext.Exit();
    }
}

[HarmonyPatchCategory(TeamPatchManager.Category)]
[HarmonyPatch(typeof(ExplosionGrenade), nameof(ExplosionGrenade.ExplodeDestructible))]
internal class GrenadeTranspiler
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> newInstructions = new(instructions);
        var index = -1;

        for (var i = 0; i < newInstructions.Count; i++)
            if (newInstructions[i].opcode == OpCodes.Call && newInstructions[i].operand is MethodInfo method &&
                method == Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetRoleId),
                    [typeof(ReferenceHub)]))
            {
                index = i;
                break;
            }

        if (index is -1 || index + 1 >= newInstructions.Count)
        {
            LogManager.Error(
                "GrenadeTranspiler could not find the expected GetRoleId call inside ExplosionGrenade.ExplodeDestructible - the method is left unpatched. Grenade friendly-fire checks may ignore fake teams.");
            return newInstructions;
        }

        newInstructions[index + 1].operand = Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam),
            [typeof(ReferenceHub)]);
        newInstructions.RemoveAt(index);

        return newInstructions;
    }
}

[HarmonyPatchCategory(TeamPatchManager.Category)]
[HarmonyPatch(typeof(PickupSearchCompletor), nameof(PickupSearchCompletor.ValidateAny))]
public class PickupSearchCompletorPatch
{
    private static bool Prefix(PickupSearchCompletor __instance, ref bool __result)
    {
        if (!DisguiseTeam.List.TryGetValue(__instance.Hub.PlayerId, out var team) || team != Team.SCPs ||
            __instance.Hub.roleManager.CurrentRole.RoleTypeId.GetTeam() == Team.SCPs) return true;
        __result = !__instance.TargetPickup.Info.Locked && !__instance.Hub.inventory.IsDisarmed() &&
                   !__instance.Hub.interCoordinator.AnyBlocker(BlockedInteraction.GrabItems);
        return false;
    }
}

[HarmonyPatchCategory(TeamPatchManager.Category)]
[HarmonyPatch]
public class DoorPermissionsPolicyPatch
{
    private static MethodBase TargetMethod()
    {
        return Method(typeof(DoorPermissionsPolicy), "CheckPermissions", [
            typeof(ReferenceHub), typeof(IDoorPermissionRequester), typeof(PermissionUsed).MakeByRefType()
        ]);
    }

    private static bool Prefix(DoorPermissionsPolicy __instance, ReferenceHub hub, IDoorPermissionRequester requester,
        out PermissionUsed callback, ref bool __result)
    {
        callback = null;
        if (__instance.RequiredPermissions == DoorPermissionFlags.None || hub.serverRoles.BypassMode)
        {
            __result = true;
            return false;
        }

        if (hub.roleManager.CurrentRole is IDoorPermissionProvider currentRole &&
            (!DisguiseTeam.List.TryGetValue(hub.PlayerId, out var team) || team != Team.SCPs))
        {
            __result = __instance.CheckPermissions(currentRole, requester, out callback);
            return false;
        }

        var curInstance = hub.inventory.CurInstance;
        __result = curInstance != null && curInstance is IDoorPermissionProvider provider &&
                   __instance.CheckPermissions(provider, requester, out callback);
        return false;
    }
}

[HarmonyPatchCategory(TeamPatchManager.Category)]
[HarmonyPatch(typeof(DoorPermissionsPolicyExtensions), nameof(DoorPermissionsPolicyExtensions.GetCombinedPermissions))]
public class DoorPermissionsPolicyExtensionsPatch
{
    private static bool Prefix(ReferenceHub hub, IDoorPermissionRequester requester, ref DoorPermissionFlags __result)
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

        var combinedPermissions = DoorPermissionFlags.None;

        if (hub.roleManager.CurrentRole is IDoorPermissionProvider currentRole &&
            (!DisguiseTeam.List.TryGetValue(hub.PlayerId, out var team) || team != Team.SCPs))
            combinedPermissions |= currentRole.GetPermissions(requester);

        var curInstance = hub.inventory.CurInstance;
        if (curInstance != null && curInstance is IDoorPermissionProvider permissionProvider)
            combinedPermissions |= permissionProvider.GetPermissions(requester);

        __result = combinedPermissions;
        return false;
    }
}

[HarmonyPatchCategory(TeamPatchManager.Category)]
[HarmonyPatch(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.IsSCP), typeof(ReferenceHub), typeof(bool))]
internal class IsScpPatch
{
    private static bool Prefix(ReferenceHub hub, ref bool __result)
    {
        if (hub == null || !DisguiseTeam.List.TryGetValue(hub.PlayerId, out var team))
            return true;

        __result = team == Team.SCPs;
        return false;
    }
}

[HarmonyPatchCategory(TeamPatchManager.Category)]
[HarmonyPatch(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.IsHuman), typeof(ReferenceHub))]
internal class IsHumanPatch
{
    private static bool Prefix(ReferenceHub hub, ref bool __result)
    {
        if (hub == null || !DisguiseTeam.List.TryGetValue(hub.PlayerId, out var team))
            return true;

        __result = team != Team.SCPs && team != Team.Dead && team != Team.Flamingos;
        return false;
    }
}

[HarmonyPatchCategory(TeamPatchManager.Category)]
[HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.OnServerRoleChanged))]
public class Scp079RecontainerPatch
{
    private static bool Prefix(Scp079Recontainer __instance, ReferenceHub hub, RoleTypeId newRole,
        RoleChangeReason reason)
    {
        var team = hub.GetRoleId().GetTeam();
        if (DisguiseTeam.List.TryGetValue(hub.PlayerId, out var t))
            team = t;
        if (newRole != RoleTypeId.Spectator || !IsScpButNot079(hub.GetRoleId(), team) ||
            Scp079Role.ActiveInstances.Count == 0 ||
            ReferenceHub.AllHubs.Any(x =>
            {
                if (x == hub)
                    return false;

                var effectiveTeam = x.GetRoleId().GetTeam();
                if (DisguiseTeam.List.TryGetValue(x.PlayerId, out var fakeTeam))
                    effectiveTeam = fakeTeam;

                return IsScpButNot079(x.GetRoleId(), effectiveTeam);
            }))
            return false;
        __instance.SetContainmentDoors(true, true);
        __instance.Recontain(true);
        foreach (var allGenerator in Scp079Recontainer.AllGenerators)
            allGenerator.Engaged = true;
        return false;
    }

    private static bool IsScpButNot079(RoleTypeId roleTypeId, Team team)
    {
        return team == Team.SCPs && roleTypeId != RoleTypeId.Scp079;
    }
}