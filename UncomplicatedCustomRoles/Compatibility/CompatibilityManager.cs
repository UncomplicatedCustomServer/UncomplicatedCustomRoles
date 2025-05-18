/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using LabApi.Loader.Features.Yaml;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Compatibility.PreviousVersionRoles;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Compatibility
{
    public class CompatibilityManager
    {
        /// <summary>
        /// Gets the location (path) of every CustomRole.
        /// </summary>
        public static Dictionary<ICustomRole, string> RolePaths { get; } = new();

        private static readonly Dictionary<Type, Version> previousVersionRoles = new()
        {
            { typeof(FossuonCustomRole), new(6, 0, 0) },
            { typeof(PreviousVersionRole), new(5, 0, 0) },
        };

        private static readonly Dictionary<ICustomRole, Version> outdatedCustomRoles = new();

        private static readonly string prefix = "[Role Loader] ";

        public static void ParseAndLoadCustomRole(string file)
        {
            string content = File.ReadAllText(file);
            CustomRole role = null;

            try
            {
                if (!TypeCheck(content, out string error))
                    throw new Exception(error);

                role = YamlConfigParser.Deserializer.Deserialize<CustomRole>(content);
            } catch (Exception ex)
            {
                // Try to decode older roles in order to make everything work
                foreach (KeyValuePair<Type, Version> kvp in previousVersionRoles)
                    try
                    {
                        object data = YamlConfigParser.Deserializer.Deserialize(content, kvp.Key);
                        if (data is IPreviousVersionRole prevRole)
                        {
                            role = prevRole.ToCustomRole();
                            outdatedCustomRoles.Add(role, kvp.Value);
                            break;
                        }
                    } 
                    catch 
                    { }

                if (role is null)
                    throw ex;
            }

            RolePaths.TryAdd(role, file);
            RegisterCustomRole(role);
        }

        public static LoadStatusType RegisterCustomRole(ICustomRole role)
        {
            LoadStatusType status = CustomRole.InternalRegister(role);

            if (status is LoadStatusType.Success)
                LogManager.Info($"{prefix}Successfully loaded CustomRole {role}!", ConsoleColor.DarkGray);
            else if (status is LoadStatusType.ValidatorError) 
            {
                CustomRole.Validate(role, out string error);
                LogManager.Error($"{prefix}Failed to load CustomRole {role}: failed to validate the CustomRole\n{error}", "RL0001");
            } 
            else if (status is LoadStatusType.SameId)
            {
                LogManager.Error($"{prefix}Failed to load CustomRole {role}: there's already another CustomRole with the same Id!", "RL0002");

                if (!RolePaths.TryGetValue(role, out string path))
                    path = null;

                if (path is not null)
                    CustomRole.NotLoadedRoles.Add(new(path, File.ReadAllLines(path), null, $"There's already another CustomRole with the Id {role.Id}"));
            }

            if (status is not LoadStatusType.SameId && outdatedCustomRoles.TryGetValue(role, out Version version))
                LogManager.Info($"{prefix}The loaded CustomRole is made for UCR v{version.ToString(3)}. Consider updating it :)", ConsoleColor.Gray);

            if (status is LoadStatusType.Success && outdatedCustomRoles.TryGetValue(role, out Version version2) && RolePaths.TryGetValue(role, out string path2))
                CustomRole.OutdatedRoles.Add(new(role, version2, path2));

            return status;
        }

        public static string GetRoleFileElement(string content, string rowPart, bool removeSpaces = true) => GetRoleFileElement(content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None), rowPart, removeSpaces);

        public static string GetRoleFileElement(string[] pieces, string rowPart, bool removeSpaces = true)
        {
            string el = pieces.FirstOrDefault(l => l.Contains(rowPart)) ?? "N/D";

            if (removeSpaces)
                el.Replace(" ", string.Empty);

            return el.Replace($"{rowPart} ", string.Empty).Replace(rowPart, string.Empty);
        }

        public static string HandleErrorString(Exception ex, bool showErrorName = false)
        {
            string message = (showErrorName ? $"{ex.GetType().Name} " : string.Empty) + ex.Message;

            if (ex.InnerException is not null)
                message += $" -> {ex.InnerException.Message}";

            if (ex.InnerException is not null && ex.InnerException.InnerException is not null)
                message += $" -> {ex.InnerException.InnerException.Message}";

            return message;
        }

        internal static int GetFirstFreeId(int start = 1)
        {
            while (CustomRole.CustomRoles.ContainsKey(start))
                start++;

            return start;
        }

        private static bool TypeCheck(string content, out string error)
        {
            error = null;
            Dictionary<string, object> data = YamlConfigParser.Deserializer.Deserialize<Dictionary<string, object>>(content);

            SnakeCaseNamingStrategy namingStrategy = new();

            foreach (PropertyInfo property in typeof(CustomRole).GetProperties().Where(p => p.CanWrite && p is not null && p.GetType() is not null))
            {
                if (!data.ContainsKey(namingStrategy.GetPropertyName(property.Name, false)))
                    error = $"Given CustomRole doesn't contain the required property '{namingStrategy.GetPropertyName(property.Name, false)}' ({namingStrategy.GetPropertyName(property.PropertyType.Name, false)})";

                if (error is not null)
                    break;
            }

            return error is null;
        }
    }
}
