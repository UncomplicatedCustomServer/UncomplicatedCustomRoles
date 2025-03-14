using CommandSystem;
using Exiled.API.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Commands
{
    public class List : IUCRCommand
    {
        public string Name { get; } = "list";

        public string Description { get; } = "List all registered custom roles";

        public string RequiredPermission { get; } = "ucr.list";

        public const int TitleSize = 23;

        public const int TextSize = 15;

        public const string Spacing = "     ";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            response = "List of all registered CustomRoles:";
            foreach (KeyValuePair<int, ICustomRole> Role in CustomRole.CustomRoles)
                if (Role.Value is not null)
                    response += $"\n<size={TitleSize}>✔ [{Role.Key}] <color={Role.Value.Role.GetColor().ToHex()}>{Role.Value?.Name}</color></size>\n    <size={TextSize}>Role: {Role.Value.Role} ({Role.Value.Team ?? Role.Value.Role.GetTeam()})\n{Spacing}HP: {Role.Value?.Health?.Amount}/{Role.Value?.Health?.Maximum}\n{Spacing}Custom info: {Role.Value?.CustomInfo}\n{Spacing}Can escape: {Role.Value.CanEscape}\n{Spacing}Inventory: {string.Join(", ", Role.Value.Inventory ?? new())}\n{Spacing}Spawn: {Role.Value?.SpawnSettings?.Spawn} [{string.Join(", ", Role.Value?.SpawnSettings?.SpawnRooms ?? new())}] [{string.Join(", ", Role.Value?.SpawnSettings?.SpawnZones ?? new())}] [{string.Join(", ", Role.Value?.SpawnSettings?.SpawnPoints ?? new())}] ({Role.Value?.SpawnSettings?.SpawnChance}%)</size>\n";

            foreach (Tuple<string, string, string, string> tuple in CustomRole.NotLoadedRoles)
                if (tuple is not null)
                    response += $"\n<size={TitleSize}>❌ [{tuple?.Item1}] <color=red>{tuple?.Item2?.Split('/')?.Last()}</color></size>\n    <size={TextSize}>Path: {tuple?.Item2}\n{Spacing}Error: <color=red>{tuple?.Item4}</color></size>\n";
            
            return true;
        }
    }
}