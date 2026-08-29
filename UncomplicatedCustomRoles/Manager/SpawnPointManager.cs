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
using System.IO;
using System.Linq;
using System.Text.Json;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Paths;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Manager;

internal static class SpawnPointManager
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public static string FilePath => Path.Combine(PathManager.Configs.FullName, $".{Server.Port}-spawnpoints.json");

    public static void Init()
    {
        if (!File.Exists(FilePath))
        {
            try
            {
                File.WriteAllText(FilePath, JsonSerializer.Serialize(Array.Empty<SpawnPoint>(), SerializerOptions));
            }
            catch (Exception e)
            {
                LogManager.Warn($"Failed to create the SpawnPoint storage file {FilePath}: {e.Message}");
                LogManager.Debug($"SpawnPointManager::Init() failed - {e}");
                return;
            }
        }

        Load();
    }

    public static int Load()
    {
        SpawnPoint.List.Clear();

        string content;

        try
        {
            content = File.ReadAllText(FilePath);
        }
        catch (FileNotFoundException)
        {
            return 0;
        }
        catch (Exception e)
        {
            LogManager.Warn($"Failed to read the SpawnPoints from {FilePath}: {e.Message}");
            LogManager.Debug($"SpawnPointManager::Load() failed - {e}");
            return 0;
        }

        List<SpawnPoint> loaded;

        try
        {
            loaded = JsonSerializer.Deserialize<List<SpawnPoint>>(content);
        }
        catch (Exception e)
        {
            SpawnPoint.List.Clear();
            LogManager.Warn(
                $"Failed to parse the SpawnPoints stored in {FilePath}: {e.Message}\nThe file is not a valid SpawnPoint list, fix it or delete it to start over.");
            LogManager.Debug($"SpawnPointManager::Load() failed - {e}");
            return 0;
        }

        if (loaded is null)
        {
            LogManager.Warn($"Failed to load the SpawnPoints: the content of {FilePath} is not a valid SpawnPoint list!");
            return 0;
        }

        if (Plugin.Instance.Config.EnableBasicLogs)
            LogManager.Info($"Loaded {loaded.Count} SpawnPoints from {FilePath}");
        else
            LogManager.Silent($"Loaded {loaded.Count} SpawnPoints from {FilePath}");

        CustomRoleSpawnCompatibilityChecker();

        return loaded.Count;
    }

    public static bool Save()
    {
        try
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(SpawnPoint.List.Where(s => s.Sync),
                SerializerOptions));
            return true;
        }
        catch (Exception e)
        {
            LogManager.Error($"Failed to store the SpawnPoints inside {FilePath}: {e.Message}");
            LogManager.Debug($"SpawnPointManager::Save() failed - {e}");
            return false;
        }
    }

    private static void CustomRoleSpawnCompatibilityChecker()
    {
        foreach (ICustomRole role in CustomRole.CustomRoles.Values.Where(role =>
                     role.SpawnSettings is not null && role.SpawnSettings.SpawnPoints is not null &&
                     role.SpawnSettings.Spawn is SpawnType.SpawnPointSpawn))
        foreach (string spawnPoint in role.SpawnSettings.SpawnPoints)
        {
            if (!SpawnPoint.Exists(spawnPoint))
            {
                LogManager.Warn(
                    $"CustomRole {role.Name} ({role.Id}) has an invalid SpawnPoint '{spawnPoint}' inside its configuration: the selected SpawnPoint does not exist!");
            }
        }
    }
}