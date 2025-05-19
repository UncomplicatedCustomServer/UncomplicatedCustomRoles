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
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Commands
{
    public class Debug : IUCRCommand
    {
        public string Name { get; } = "debug";

        public string Description { get; } = "Debug the plugin by using some specific code";

        public string RequiredPermission { get; } = "ucr.debug";

        private object ReferenceObject { get; set; } = null;

        public bool Executor(List<string> args, ICommandSender sender, out string response)
        {
            response = null;

            if (args.Count < 3)
            {
                response = "Usage: ucr debug <Target> <Location> <Value> [saveToRef?]";
                return false;
            }

            object obj = null;
            Type target = args[0] is "static" ? Plugin.Instance.Assembly.GetType(args[1]) : null;

            if (target is null && args[0] is "static")
            {
                response = $"Failed to start debug: Location '{args[1]}' is not valid!";
                return false;
            }

            Player player = Player.Get(sender);

            if (args[0] is "plugin" or "pl")
                obj = Plugin.Instance;
            if (args[0] is "current_player_scr" or "cp_scr" && player is not null)
                obj = player.GetSummonedInstance();
            else if (args[0] is "current_player" or "cp" && player is not null)
                obj = player;
            else if (args[0].StartsWith("player_") && int.TryParse(args[0].Replace("player_", string.Empty), out int id) && Player.TryGet(id, out Player player3))
                obj = player3;
            else if (args[0].StartsWith("player_scr_") && int.TryParse(args[0].Replace("player_scr_", string.Empty), out int id2) && Player.TryGet(id2, out Player player4))
                obj = player4.GetSummonedInstance();
            else if (args[0].StartsWith("current_player_cm_") && player.TryGetSummonedInstance(out SummonedCustomRole role) && role.CustomModules.FirstOrDefault(cm => cm.Name == args[0].Replace("current_player_cm_", "")) is not null)
                obj = role.CustomModules.FirstOrDefault(cm => cm.Name == args[0].Replace("current_player_cm_", ""));
            else if (args[0] is "ref" or "reference")
                obj = ReferenceObject;

            if (obj is null && args[0] is not "static") 
            {
                response = $"Failed to start debug: Zone {args[0]} not found!";
                return false;
            }

            if (args.Count is 4 && args[3] is "true" && args[0] is not "ref" or "reference")
                ReferenceObject = obj;

            if (obj is not null && target is not null && obj.GetType() != target)
            {
                response = $"Failed to start debug: Given target of debug {target.FullName} is not equal to the found object {obj.GetType().FullName}!";
                return false;
            }

            if (obj is null && target is null)
            {
                response = $"Failed to start debug: Both target and object cannot be null!\nIn order to start the debug of a static element, PLEASE give the target (Location) of it!";
                return false;
            }

            target ??= obj.GetType();

            if (HandleProperty(obj, target, args[2], out string value))
                response = $"Required value is:\n{value}";
            else if (HandleField(obj, target, args[2], out value))
                response = $"Required value is:\n{value}";

            if (response is not null)
                return true;

            response = $"Failed to find value: Element at {target.FullName}.{args[2]} not found!";
            return false;
        }

        private bool HandleProperty(object obj, Type target, string name, out string value)
        {
            PropertyInfo property;

            if (obj is null)
                property = target.GetProperties(BindingFlags.Static).FirstOrDefault(p => p.Name == name && p.CanRead);
            else
                property = target.GetProperties().FirstOrDefault(p => p.Name == name && p.CanRead);

            if (property is null)
            {
                value = null;
                return false;
            }

            value = property.GetValue(obj).ToString();
            return true;
        }

        private bool HandleField(object obj, Type target, string name, out string value)
        {
            FieldInfo field;

            if (obj is null)
                field = target.GetFields().FirstOrDefault(f => f.Name == name && f.IsStatic);
            else
                field = target.GetFields().FirstOrDefault(f => f.Name == name);

            if (field is null)
            {
                value = null;
                return false;
            }

            value = field.GetValue(obj).ToString();
            return true;
        }
    }
}
