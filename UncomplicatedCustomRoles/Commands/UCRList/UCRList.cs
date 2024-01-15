using CommandSystem;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Structures;

namespace UncomplicatedCustomRoles.Commands.UCRList
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class UCRList : ParentCommand
    {
        public UCRList() => LoadGeneratedCommands();

        public override string Command { get; } = "ucrlist";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "List all registered custom roles";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!((CommandSender)sender).CheckPermission("ucr.list"))
            {
                response = "You do not have permission to use this command";
                return false;
            }

            if (arguments.Count != 0)
            {
                response = "No args needed here!";
                return false;
            }

            response = "List of all registered CustomRoles:";
            foreach (KeyValuePair<int, ICustomRole> Role in Plugin.CustomRoles)
            {
                response += $"\n{Role.Key} -> {Role.Value.Name} ({Role.Value.Role})";
            }

            return true;
        }
    }
}