using Exiled.API.Features;
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
