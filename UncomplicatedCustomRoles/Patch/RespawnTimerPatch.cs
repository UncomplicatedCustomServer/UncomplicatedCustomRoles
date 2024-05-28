using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using HarmonyLib;
using PlayerRoles;
using Respawning;
using RespawnTimer.API.Features;
using UncomplicatedCustomRoles;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Interfaces;

namespace UncomplicatedCustomRolesRespawnTimer;

public static class RespawnTimerPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(TimerView), nameof(TimerView.GetText))]
    public static bool GetText(TimerView __instance, ref string __result, int? spectatorCount = null)
    {
        __instance.StringBuilder.Clear();
        __instance.StringBuilder.Append(
            RespawnManager.Singleton._curSequence == RespawnManager.RespawnSequencePhase.PlayingEntryAnimations 
            ? __instance.DuringRespawnString 
            : __instance.BeforeRespawnString);
        __instance.SetAllProperties(spectatorCount);
        __instance.StringBuilder.Replace("{RANDOM_COLOR}", $"#{UnityEngine.Random.Range(0, 16777215):X6}");
        __result = __instance.StringBuilder.ToString();
        return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(RespawnTimer.EventHandler), "TimerCoroutine", MethodType.Enumerator)]
    public static IEnumerable<CodeInstruction> TimerCoroutine(IEnumerable<CodeInstruction> instructions, MethodBase method)
    {
        var codesMatcher = new CodeMatcher(instructions);
        
        var propCurrent = AccessTools.PropertyGetter(typeof(IEnumerator<Player>), "Current");
        codesMatcher.MatchEndForward(
            new CodeMatch(OpCodes.Callvirt, propCurrent)
            );

        if (codesMatcher.ReportFailure(method, Log.Error))
            return instructions;

        codesMatcher.Advance(1).ThrowIfNotMatch(
            "Can not find the variable use to store the Player durring foreach",
            new CodeMatch(OpCodes.Stloc_S)
            );
        
        var playerVar = codesMatcher.Operand;

        TimerView TimerView = null; // just use for the typo
        codesMatcher.Start().MatchEndForward(
            CodeMatch_Ctor(() => TimerView.GetText(0))
            );

        if (codesMatcher.ReportFailure(method, Log.Error))
            return instructions;

        codesMatcher.Advance(1).ThrowIfNotMatch(
            "Can not find the variable use to store the result of GetText",
            new CodeMatch(OpCodes.Stloc_S)
            );

        var textVar = codesMatcher.Operand;

        Player Player = null;
        codesMatcher.MatchStartForward(
            CodeMatch_Ctor(() => Player.ShowHint(string.Empty, 0))
            );

        if (codesMatcher.ReportFailure(method, Log.Error))
            return instructions;

        // Why 3 ? bc one for the "this" (Player), one for the "text" (string)
        // and one for duration (float). If this is changed edit it here.
        codesMatcher.Advance(-3);
        codesMatcher.Insert(
            new CodeInstruction(OpCodes.Ldloc_S, textVar),
            new CodeInstruction(OpCodes.Ldloc_S, playerVar),
            CodeInstruction.Call(() => SetupRoleProperty(string.Empty, Player)),
            new CodeInstruction(OpCodes.Stloc_S, textVar)
            );

        return codesMatcher.Instructions();
    }

    public static string SetupRoleProperty(string text, Player player)
    {
        if (player.Role is not SpectatorRole spectator) return text;

        var spectated = spectator.SpectatedPlayer;
        string roleName;

        if (spectated == null)
        {
            roleName = "...";
        } 
        else if (spectated.TryGetCustomRole(out var customRole))
        {
            roleName = GetCustomRoleName(customRole, player);
        }
        else
        {
            roleName = spectated.Role.Name;
        }
        text = text.Replace("{role}", roleName);
        text = text.Replace('{', '[').Replace('}', ']');
        return text;
    }

    public static string GetCustomRoleName(ICustomRole role, Player watcherPlayer)
    {
        if (!Plugin.Instance.Config.HiddenRolesId.TryGetValue(role.Id, out var information))
            return role.Name;

        if (information.OnlyVisibleOnOverwatch)
        {
            if (watcherPlayer.Role == RoleTypeId.Overwatch)
            {
                return role.Name;
            }
        }
        else
        {
            if (watcherPlayer.RemoteAdminAccess)
            {
                return role.Name;
            }
        }
        return information.RoleNameWhenHidden;
    }

    // bug with this version of harmony. Do not use new CodeMatch("lambda method (with no param) or expression").
    // https://github.com/pardeike/Harmony/blob/11f3a1de4c512f9da39fed8a15fc1e8f5fa397a3/Harmony/Tools/CodeMatch.cs#L102
    public static CodeMatch CodeMatch_Ctor(LambdaExpression expression, string name = null)
    {
        var codeMatch = new CodeMatch();
        codeMatch.operand = SymbolExtensions.GetMethodInfo(expression);
        if (codeMatch.operand != null)
            codeMatch.operands.Add(codeMatch.operand);
        codeMatch.name = name;
        return codeMatch;
    }


}
