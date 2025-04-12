using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Compatibility.PreviousVersionRoles;
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
            { typeof(PreviousVersionRole), new(5, 0, 0) },
            { typeof(FossuonCustomRole), new(6, 0, 0) }
        };

        private static readonly Dictionary<ICustomRole, Version> outdatedCustomRoles = new();

        private static readonly string prefix = "[Role Loader] ";

        public static void ParseAndLoadCustomRole(string file)
        {
            string content = File.ReadAllText(file);
            CustomRole role = null;

            try
            {
                role = Loader.Deserializer.Deserialize<CustomRole>(content);
            } catch (Exception ex)
            {
                // Try to decode older roles in order to make everything work
                foreach (KeyValuePair<Type, Version> kvp in previousVersionRoles)
                    try
                    {
                        object data = Loader.Deserializer.Deserialize(content, kvp.Key);
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

            RegisterCustomRole(role);
        }

        public static LoadStatusType RegisterCustomRole(ICustomRole role)
        {
            LoadStatusType status = CustomRole.Register(role);

            if (status is LoadStatusType.Success)
                LogManager.Info($"{prefix}Successfully loaded CustomRole {role}!", ConsoleColor.DarkGray);
            else if (status is LoadStatusType.ValidatorError)
                LogManager.Error($"{prefix}Failed to load CustomRole {role}: failed to validate the CustomRole", "RL0001");
            else if (status is LoadStatusType.SameId)
            {
                LogManager.Error($"{prefix}Failed to load CustomRole {role}: there's already another CustomRole with the same Id!", "RL0002"); 
                if (!RolePaths.TryGetValue(role, out string path))
                    path = string.Empty;
                CustomRole.NotLoadedRoles.Add(new(role.Id.ToString(), path, "AlreadyHereException", $"There's already another CustomRole with the Id {role.Id}"));
            }

            if (status is not LoadStatusType.SameId && outdatedCustomRoles.TryGetValue(role, out Version version))
                LogManager.Info($"{prefix}The loaded CustomRole is made for UCR v{version.ToString(3)}. Consider updating it :)", ConsoleColor.Gray);

            return status;
        }

        internal static int GetFirstFreeId(int start = 1)
        {
            while (CustomRole.CustomRoles.ContainsKey(start))
                start++;

            return start;
        }

        public static string GetRoleFileId(string content) => GetRoleFileId(content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));

        public static string GetRoleFileId(string[] pieces) => (pieces.FirstOrDefault(l => l.Contains("id:")) ?? "N/D").Replace(" ", "").Replace("id:", "");
    }
}
