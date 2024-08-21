using CommandSystem;
using Exiled.Loader;
using System.Collections.Generic;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Commands
{
    internal class Show : IUCRCommand
    {
        public string Name { get; } = "show";

        public string Description { get; } = "Show a specific CustomRole if loaded";

        public string RequiredPermission { get; } = "ucr.show";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count != 1)
            {
                response = "Usage: ucr show (Role Id)";
                return false;
            }

            if (int.TryParse(arguments[0], out int id))
            {
                if (CustomRole.CustomRoles.ContainsKey(id))
                    response = $"Showing CustomRole {id}\n\n{Loader.Serializer.Serialize(CustomRole.CustomRoles[id])}";
                else
                    response = $"CustomRole {id} is not registered!";
                return true;
            }

            response = "Failed to parse the Id of the CustomRole!\nUsage: ucr show (Role Id)";
            return false;
        }
    }
}
