/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UnityEngine;

namespace UncomplicatedCustomRoles.Manager
{
    public class PlaceholderManager
    {
        public static string ApplyPlaceholders(string origin, Player player, ICustomRole role) => origin.BulkReplace(new()
            {
                { "nick", player.Nickname },
                { "displayname", player.DisplayNickname },
                { "rand", Random.Range(0, 10) },
                { "dnumber", Random.Range(1000, 10000) },
                { "unitid", player.UnitId },
                { "unitname", player.UnitName },
                { "rolename", player.Role.Name },
                { "customrolename", role.Name },
                { "customroleid", role.Id },
                { "customrolebadge", role.BadgeName },
            }, "%<val>%");
    }
}
