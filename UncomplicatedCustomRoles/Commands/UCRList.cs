using CommandSystem;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Interfaces;

namespace UncomplicatedCustomRoles.Commands
{
    public class UCRList : IUCRCommand
    {
        public string Name { get; } = "list";

        public string Description { get; } = "List all registered custom roles";

        public string RequiredPermission { get; } = "ucr.list";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            response = "List of all registered CustomRoles:";
            foreach (KeyValuePair<int, ICustomRole> Role in Plugin.CustomRoles)
            {
                response += $"\n[{Role.Key}] '{Role.Value.Name}' ({Role.Value.Role})";
            }

            return true;
        }
    }
}