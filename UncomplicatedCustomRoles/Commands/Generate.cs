using CommandSystem;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Interfaces;

namespace UncomplicatedCustomRoles.Commands
{
    internal class Generate : IUCRCommand
    {
        public string Name { get; } = "generate";

        public string Description { get; } = "Generate another default CustomRole inside a given file, creating it";

        public PlayerPermissions RequiredPermission { get; } = PlayerPermissions.LongTermBanning;

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count == 0)
            {
                response = "Unexpected number of args!\nUsage: ucr generate (FileName) (port)";
                return false;
            }

            int port = -1;
            if (arguments.Count == 2)
                port = (int)uint.Parse(arguments[1]);

            string path = Plugin.FileConfigs.Dir;
            if (port > 0)
                path = Path.Combine(path, port.ToString());

            Path.Combine(path, $"{arguments[0].Replace(".yml", "")}.yml");

            File.WriteAllText(path, JsonConvert.SerializeObject(new CustomRole()));

            response = $"New default role generated at {path} but has not been loaded!";
            return true;
        }
    }
}
