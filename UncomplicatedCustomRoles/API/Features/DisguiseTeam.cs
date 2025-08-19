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

        public static bool IsEnemy(Footprint attacker, ReferenceHub ply)
        {
            try
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
            }
        }

        public static bool @Handler1(Footprint attacker)
        {
            if (List.TryGetValue(attacker.PlayerId,out Team team))
                return team != Team.SCPs;

            return attacker.Role.GetTeam() != Team.SCPs;
        }
    }
}