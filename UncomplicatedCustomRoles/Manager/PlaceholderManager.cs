/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using LabApi.Features.Wrappers;
using Respawning.NamingRules;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UnityEngine;

namespace UncomplicatedCustomRoles.Manager;
#nullable enable

public class PlaceholderManager
{
    public static string ApplyPlaceholders(string? origin, Player player, ICustomRole? role)
    {
        return (origin ?? string.Empty).BulkReplace(new Dictionary<string, object>
        {
            { "nick", player.Nickname },
            { "displayname", player.DisplayName },
            { "rand", Random.Range(0, 10) },
            { "dnumber", Random.Range(1000, 10000) },
            { "unitid", player.UnitId },
            {
                "unitname",
                NamingRulesManager.TryGetNamingRule(player.Team, out var namingRule)
                    ? namingRule.LastGeneratedName
                    : string.Empty
            },
            { "rolename", player.Role.GetFullName() },
            { "customrolename", role?.Name },
            { "customroleid", role?.Id },
            { "customrolebadge", role?.BadgeName },
            { "health", player.Health },
            { "max_health", player.MaxHealth },
            { "ahp", player.ArtificialHealth },
            { "max_ahp", player.MaxArtificialHealth },
            { "hume", player.HumeShield },
            { "max_hume", player.MaxHumeShield }
        }, "%<val>%");
    }
}