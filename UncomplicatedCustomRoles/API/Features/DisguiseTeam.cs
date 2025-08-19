using Footprinting;
using PlayerRoles;
using System.Collections.Concurrent;

namespace UncomplicatedCustomRoles.API.Features
{
    public class DisguiseTeam
    {
        public static readonly ConcurrentDictionary<int, Team> List = new();

        public static readonly ConcurrentDictionary<int, PlayerRoleBase> RoleBaseList = new();

        public static bool IsEnemy(object attacker, object ply)
        {
            return true;
            /*Team attackerTeam = attacker.Role.GetTeam();
            Team playerTeam = ply.GetTeam();

            List.TryGetValue(attacker.PlayerId, out attackerTeam);

            return attackerTeam != playerTeam;*/
        }

        public static bool @Handler1(Footprint attacker)
        {
            if (List.TryGetValue(attacker.PlayerId,out Team team))
                return team != Team.SCPs;

            return attacker.Role.GetTeam() != Team.SCPs;
        }
    }
}