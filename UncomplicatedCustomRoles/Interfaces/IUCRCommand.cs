using CommandSystem;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.Interfaces
{
    internal interface IUCRCommand
    {
        public string Name { get; }

        public string Description { get; }

        public PlayerPermissions RequiredPermission { get; }

        public bool Executor(List<string> arguments, ICommandSender sender, out string response);
    }
}
