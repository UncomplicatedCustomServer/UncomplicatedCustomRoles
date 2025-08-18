/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using InventorySystem;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Commands
{
    public class Info : IUCRCommand
    {
        public string Name { get; } = "info";

        public string Description { get; } = "View info about a specific Custom Role";

        public string RequiredPermission { get; } = "ucr.info";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count != 1)
            {
                response = "Usage: ucr info <Role Id>";
                return false;
            }

            if (!int.TryParse(arguments[0], out int id) || !CustomRole.TryGet(id, out ICustomRole role))
            {
                response = $"Custom Role {arguments[0]} not found!";
                return false;
            }

            response = $"<size=23><b>{role.Name}</b></size>";

            response += BuildInfo(role);

            response += $"\n<size=1>OwO</size>";

            return true;
        }

        public static string BuildInfo(ICustomRole role)
        {
            Dictionary<string, string> data = new()
            {
                { "<color=#00ffff>🔢</color> Id:", $"<b>{role.Id}</b>" },
                { "<color=#757575>👤</color> Role:", $"<color={role.Role.GetColor().ToHex()}><b>{role.Role}</b></color>" },
                { "<color=#459426>💳</color> Badge:", $"<color={SpawnManager.colorMap[role.BadgeColor] ?? "white"}>{role.BadgeName.Replace("@hidden", string.Empty)}</color>{(role.BadgeName.EndsWith("@hidden") ? " [HIDDEN]" : string.Empty)}" },
                { "<color=#ff0000>❤️</color> Health:", $"<b>{role?.Health.Amount ?? 0}</b>/{role?.Health.Maximum ?? 0}" },
                { "<color=#00ff00>💉</color> AHP:", $"<b>{role?.Ahp.Amount ?? 0}</b>/{role?.Ahp.Limit ?? 0}" },
                { "<color=#88c460>🏃</color> Can escape:", $"<b>{(role.CanEscape ? "true" : "false")}</b>" },
                { "<color=#caeded>🎒</color> Inventory:", string.Join(", ", role?.Inventory ?? new List<ItemType>()) },
                { "<color=#a61c1c>🚗</color> Spawn type:", $"<b>{role.SpawnSettings?.Spawn}</b>" }
            };

            string response = string.Empty;

            if (role.SpawnSettings is not null)
                if (role.SpawnSettings.Spawn is SpawnType.RoomsSpawn)
                    data.Add("<color=#632300>🚪</color> Spawn rooms:", string.Join(", ", role?.SpawnSettings?.SpawnRooms ?? new List<RoomType>()));
                else if (role.SpawnSettings.Spawn is SpawnType.ZoneSpawn)
                    data.Add("<color=#632300>🚪</color> Spawn zones:", string.Join(", ", role?.SpawnSettings?.SpawnZones ?? new List<ZoneType>()));
                else if (role.SpawnSettings.Spawn is SpawnType.SpawnPointSpawn)
                    data.Add("<color=#632300>🚪</color> Spawn points:", string.Join(", ", role?.SpawnSettings?.SpawnPoints ?? new List<string>()));

            if (role.CustomFlags is not null && role.CustomFlags.Count > 0)
                data.Add("<color=#bf4eb6>🧩</color> Custom flags:", string.Join(", ", YamlFlagsHandler.Decode(role.CustomFlags).Keys));

            foreach (KeyValuePair<string, string> kvp in data)
                response += $"\n{kvp.Key.GenerateWithBuffer(40)} {kvp.Value}";

            return response;
        }
    }
}
