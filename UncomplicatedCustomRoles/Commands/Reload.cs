using CommandSystem;
using Exiled.API.Features;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Interfaces;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Commands
{
#pragma warning disable CS0618 // Obsolete
    public class Reload : IUCRCommand
    {
        public string Name { get; } = "reload";

        public string Description { get; } = "Reload every custom role loaded and search for new";

        public string RequiredPermission { get; } = "ucr.reload";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (!Round.IsStarted)
            {
                response = "Sorry but you can't use this command if the round is not started!";
                return false;
            }

            // Create a copy of the custom roles Dictionary
            Dictionary<int, ICustomRole> Roles = new();

            Plugin.FileConfigs.LoadAction((CustomRole Role) =>
            {
                if (!CustomRole.Validate(Role))
                {
                    LogManager.Warn($"[RL] Failed to register the UCR role with the ID {Role.Id} due to the validator check!");
                    return;
                }

                if (!Roles.ContainsKey(Role.Id))
                {
                    Roles.Add(Role.Id, Role);

                    if (Plugin.Instance.Config.EnableBasicLogs)
                    {
                        LogManager.Info($"[RL] Successfully registered the UCR role with the ID {Role.Id} and {Role.Name} as name!");
                    }

                    return;
                }

                LogManager.Warn($"[RL] Failed to register the UCR role with the ID {Role.Id}: apparently there's already another role with the same Id!\nId fixer deactivated [!]");
            });

            Plugin.FileConfigs.LoadAction((CustomRole Role) =>
            {
                if (!API.Features.CustomRole.Validate(Role))
                {
                    LogManager.Warn($"[RL] Failed to register the UCR role with the ID {Role.Id} due to the validator check!");
                    return;
                }

                if (!Roles.ContainsKey(Role.Id))
                {
                    Roles.Add(Role.Id, Role);

                    if (Plugin.Instance.Config.EnableBasicLogs)
                    {
                        LogManager.Info($"[RL] Successfully registered the UCR role with the ID {Role.Id} and {Role.Name} as name!");
                    }

                    return;
                }

                LogManager.Warn($"[RL] Failed to register the UCR role with the ID {Role.Id}: apparently there's already another role with the same Id!\nId fixer deactivated [!]");
            }, Server.Port.ToString());

            if (Roles.Count < CustomRole.CustomRoles.Count)
            {
                response = $"The reload command found a role that is loaded in the plugin but has not been loaded by the reload!\nYou can't remove custom roles without restarting the server!\nExpected {CustomRole.CustomRoles.Count} roles, found {Roles.Count}";
                return true;
            }

            foreach (ICustomRole Role in CustomRole.List)
            {
                if (!Roles.ContainsKey(Role.Id))
                {
                    response = $"The reload command found a role that is loaded in the plugin but has not been loaded by the reload!\nYou can't remove custom roles without restarting the server!\nMissing role: {Role.Id}";
                    return true;
                }
            }

            // Ok now we can push the dictionary
            CustomRole.CustomRoles = Roles;

            response = $"\n>> UCR Reload Report <<\nReloaded {Roles.Count} custom roles.\nFound {CustomRole.CustomRoles.Count - Roles.Count} new roles.\n⚠ WARNING ⚠\nIf you have modified something like the health or the name the players that currently have this custom roles won't be affected by these changes!";
            return true;
        }
    }
}