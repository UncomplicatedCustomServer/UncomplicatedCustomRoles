using Footprinting;
using PlayerRoles;
using System;
using System.Collections.Concurrent;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features
{
    public class DisguiseTeam
    {
        public static readonly ConcurrentDictionary<int, Team> List = new();

        public static readonly ConcurrentDictionary<int, PlayerRoleBase> RoleBaseList = new();

        public static bool IsEnemy(/*Footprint attacker, ReferenceHub ply*/object attacker, object ply)
        {
            if (attacker is not null)
                LogManager.Info($"[RL] {attacker.GetType().FullName} [1]");

            if (ply is not null)
                LogManager.Info($"[RL] {attacker.GetType().FullName} [1]");

            LogManager.Info("WORK");

            return false;
            /*try
            {
                Team attackerTeam = attacker.Role.GetTeam();
                Team playerTeam = ply.GetTeam();

                if (List.TryGetValue(attacker.PlayerId, out Team _att))
                    attackerTeam = _att;

                return attackerTeam != playerTeam;
            } catch (Exception ex)
            {
                LogManager.Error(ex.ToString());
                return false;
            }*/
        }

        public static bool @Handler1(Footprint attacker)
        {
            if (List.TryGetValue(attacker.PlayerId,out Team team))
                return team != Team.SCPs;

            return attacker.Role.GetTeam() != Team.SCPs;
        }
    }
}
