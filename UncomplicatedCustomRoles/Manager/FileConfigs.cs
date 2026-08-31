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
using System.IO;
using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Yaml;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Compatibility;

namespace UncomplicatedCustomRoles.Manager;

internal static class FileConfigs
{
    internal static string Dir = Path.Combine(PathManager.Configs.FullName, "UncomplicatedCustomRoles");

    public static bool Is(string localDir = "")
    {
        return Directory.Exists(Path.Combine(Dir, localDir));
    }

    public static string[] List(string localDir = "")
    {
        return Directory.GetFiles(Path.Combine(Dir, localDir));
    }

    public static void LoadAll(string localDir = "")
    {
        LoadAction(localDir);

        foreach (string dir in Directory.GetDirectories(Path.Combine(Dir, localDir)))
        {
            string name = dir.Replace(Dir, string.Empty);
            if (name[0] is '/' or '\\')
                name = name.Remove(0, 1);

            if (int.TryParse(name, out int num) && num < 990000)
                continue;

            if (name is "")
                continue;

            LoadAction(name);
        }
    }

    public static void LoadAction(string localDir = "")
    {
        foreach (string FileName in List(localDir))
            try
            {
                if (Directory.Exists(FileName))
                    continue;

                if (Path.GetFileName(FileName).StartsWith("."))
                    continue;

                if (FileName.EndsWith(".dll"))
                {
                    PluginImportManager.Load(FileName);
                    continue;
                }

                CompatibilityManager.ParseAndLoadCustomRole(FileName);
            }
            catch (Exception ex)
            {
                string[] fileLines;
                try
                {
                    fileLines = File.ReadAllLines(FileName);
                }
                catch
                {
                    fileLines = [];
                }

                CustomRole.NotLoadedRoles.Add(new ErrorCustomRole(FileName, fileLines, ex));

                if (!Plugin.Instance.Config.Debug)
                    LogManager.Error($"Failed to parse {FileName}:\n{CompatibilityManager.HandleErrorString(ex, true)}\nNotice: This YAML error has been caused by this configuration and it's not a bug of the whole plugin!", "SR0001");
                else
                    LogManager.Error($"Failed to parse {FileName}. YAML Exception: {ex.Message}.\nStack trace: {ex.StackTrace}\nThis is a YAML error that YOU CAUSED and therefore >>YOU<< NEED TO FIX IT!\nDON'T COME TO US WITH THIS ERROR!", "SR0001");
            }
    }

    public static void Welcome(string localDir = "")
    {
        if (!Is(localDir))
        {
            Directory.CreateDirectory(Path.Combine(Dir, localDir));
            File.WriteAllText(Path.Combine(Dir, localDir, "example-role.yml"), YamlConfigParser.Serializer.Serialize(new CustomRole
            {
                Id = CompatibilityManager.GetFirstFreeId()
            }));

            LogManager.Info($"Plugin does not have a role folder, generated one in {Path.Combine(Dir, localDir)}");
        }
    }
}