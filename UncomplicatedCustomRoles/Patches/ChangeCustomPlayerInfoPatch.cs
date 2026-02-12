using System;
using System.Collections.Generic;
using System.Text;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using HarmonyLib;
using LabApi.Features.Wrappers;
using NorthwoodLib.Pools;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using Utils;

namespace UncomplicatedCustomRoles.Patches;

[HarmonyPatch(typeof(ChangeCustomPlayerInfoCommand), nameof(ChangeCustomPlayerInfoCommand.Execute))]
internal class ChangeCustomPlayerInfoPatch
{
    private static bool Prefix(ChangeCustomPlayerInfoCommand __instance, ArraySegment<string> arguments, ICommandSender sender,  out string response, ref bool __result)
    {
        if (!sender.CheckPermission(PlayerPermissions.PlayersManagement, out response))
        {
            __result = false;
            return false;
        }
        if (arguments.Count < 1)
        {
            response = $"To execute this command provide at least 1 argument!\nUsage: {arguments.Array[0]} {__instance.DisplayCommandUsage()}";
            __result = false;
            return false;
        }
        string[] newargs;
        List<ReferenceHub> referenceHubList = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out newargs);
        if (referenceHubList == null)
        {
            response = "Cannot find player! Try using the player ID!";
            __result = false;
            return false;
        }
        string str = newargs == null ? (string) null : string.Join(" ", newargs);
        StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
        foreach (ReferenceHub me in referenceHubList)
        {
            var player = Player.Get(me);
            if (str == null)
            {
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, $"{sender.LogName} cleared custom info of player {me.PlayerId} ({me.nicknameSync.MyNick}).", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                stringBuilder.AppendFormat("Reset {0}'s custom info.\n", (object) me.LoggedNameFromRefHub());
                if (player.TryGetSummonedInstance(out SummonedCustomRole summonedInstance))
                    player.RefreshInfoArea(summonedInstance.Role.CustomInfo);
                else
                    me.nicknameSync.CustomPlayerInfo = null;
            }
            else
            {
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, $"{sender.LogName} set custom info of player {me.PlayerId} ({me.nicknameSync.MyNick}) to \"{str}\".", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                stringBuilder.AppendFormat("Set {0}'s custom info to: {1}\n", (object) me.LoggedNameFromRefHub(), (object) str);
                if (player.HasCustomRole())
                    player.RefreshInfoArea(str);
                else
                    me.nicknameSync.CustomPlayerInfo = str;
            }
        }
        response = stringBuilder.ToString().Trim();
        StringBuilderPool.Shared.Return(stringBuilder);
        __result = true;
        return false;
    }
}