/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using MapGeneration;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using SpawnPoint = UncomplicatedCustomRoles.API.Features.SpawnPoint;

namespace UncomplicatedCustomRoles.Manager;

internal static class MapSpawnValidator
{
    internal static void ValidateAll()
    {
        foreach (var role in CustomRole.CustomRoles.Values)
            RoleValidator.ValidatePostLoad(role);

        var rooms = Room.List;
        if (rooms is null || rooms.Count == 0)
            return;

        HashSet<string> roomNames = new(
            rooms.Where(r => r?.GameObject is not null).Select(r => r.GameObject.name.RemoveBracketsOnEndOfName()),
            StringComparer.Ordinal);

        HashSet<FacilityZone> zonesWithRooms = new(rooms.Select(r => r.Zone));

        var loggedValidRooms = false;

        foreach (var role in CustomRole.CustomRoles.Values)
        {
            var spawn = role.SpawnSettings;
            if (spawn is null)
                continue;

            var label = $"{role.Name} ({role.Id})";

            switch (spawn.Spawn)
            {
                case SpawnType.RoomsSpawn when spawn.SpawnRooms is not null:
                    foreach (var roomName in spawn.SpawnRooms.Where(name => !roomNames.Contains(name)))
                    {
                        LogManager.Warn(
                            $"[Role Validator] {label}: spawn room '{roomName}' does not exist on the current map; players there fall back to their original position.");
                        if (!loggedValidRooms)
                        {
                            LogManager.Warn(
                                $"[Role Validator] Rooms available on the current map: {string.Join(", ", roomNames.OrderBy(n => n))}");
                            loggedValidRooms = true;
                        }
                    }

                    break;

                case SpawnType.SpawnPointSpawn when spawn.SpawnPoints is not null:
                    foreach (var pointName in spawn.SpawnPoints.Where(name => !SpawnPoint.Exists(name)))
                        LogManager.Warn(
                            $"[Role Validator] {label}: spawn point '{pointName}' is not registered. Registered spawn points: {RegisteredSpawnPoints()}.");
                    break;

                case SpawnType.ZoneSpawn when spawn.SpawnZones is not null:
                    foreach (var zone in spawn.SpawnZones.Where(z => !zonesWithRooms.Contains(z)))
                        LogManager.Warn(
                            $"[Role Validator] {label}: zone '{zone}' has no rooms on the current map, players can't be placed there.");
                    break;
            }
        }
    }

    private static string RegisteredSpawnPoints()
    {
        var names = SpawnPoint.List.Concat(SpawnPoint.UnsyncedList).Select(p => p.Name);
        var enumerable = names.ToList();
        return enumerable.Any() ? string.Join(", ", enumerable) : "(none)";
    }
}