/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using CommandSystem;
using LabApi.Loader.Features.Yaml;
using System.Collections.Generic;
using System.IO;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Commands
{
    internal class Generate : IUCRCommand
    {
        public string Name { get; } = "generate";

        public string Description { get; } = "Generate another default Custom Role inside a given file, creating it";

        public string RequiredPermission { get; } = "ucr.generate";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count == 0)
            {
                response = "Unexpected number of args!\nUsage: ucr generate (FileName) (Server-port)";
                return false;
            }

            int port = -1;
            if (arguments.Count == 2)
                port = (int)uint.Parse(arguments[1]);

            string path = FileConfigs.Dir;
            if (port > 0)
                path = Path.Combine(path, port.ToString());

            File.WriteAllText(Path.Combine(path, $"{arguments[0].Replace(".yml", "")}.yml"), YamlConfigParser.Serializer.Serialize(new CustomRole()));

            response = $"New default role generated at {path} but has not been loaded!";
            return true;
        }
    }
}
