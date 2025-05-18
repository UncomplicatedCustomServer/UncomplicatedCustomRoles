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
using System.Collections.Generic;
using System.IO;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Compatibility;
using YamlDotNet.Core;

namespace UncomplicatedCustomRoles.Commands
{
    public class Errors : IUCRCommand
    {
        public string Name { get; } = "errors";

        public string Description { get; } = "See every YAML error of every not loaded CustomRole";

        public string RequiredPermission { get; } = "ucr.errors";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (CustomRole.NotLoadedRoles.Count is 0)
            {
                response = "No CustomRoles with errors were found!\nYey :3";
                return true;
            }

            response = string.Empty;

            foreach (ErrorCustomRole errorCustomRole in CustomRole.NotLoadedRoles)
            {
                response += $"\n<color=#FFFFFF>📄</color> <b>File:</b> {Path.GetFileName(errorCustomRole.Path)}";

                if (errorCustomRole.Exception is YamlException yamlException)
                    response += $"\n<color=#00FFFF>🔢</color> Line: {yamlException.Start.Line}, Column: {yamlException.Start.Column}";

                response += $"\n<color=red>❌</color> Error: {errorCustomRole.Message}";
                response += $"\n<color=#FFFF00>💡</color> Suggestion: {(errorCustomRole.Exception is not null && errorCustomRole.Exception.Message is not null ? GetSuggestionFromMessage(errorCustomRole.Exception.Message) : string.Empty)}\n";
            }

            return true;
        }

        private static string GetSuggestionFromMessage(string message)
        {
            message = message.ToLowerInvariant();

            if (message.Contains("mapping values are not allowed"))
                return "Make sure there is a space after the colon (e.g., `name: GOC` instead of `name:GOC`).";

            if (message.Contains("expected 'mappingstart', got 'sequencestart'"))
                return "Your YAML file begins with a list (`- item`) but should begin with a mapping. Try adding a top-level key like `teams:` before your list.";

            if (message.Contains("while parsing a block mapping"))
                return "Check indentation and YAML structure — something might be misaligned or nested incorrectly.";

            if (message.Contains("expected <block end>, but found"))
                return "Possibly missing a `-` for a list item or the element ends prematurely.";

            if (message.Contains("did not find expected key"))
                return "A key may be missing or misaligned — ensure all keys are followed by colons and correctly indented.";

            if (message.Contains("unexpected end of stream"))
                return "The file might be cut off unexpectedly — check for missing closing brackets or incomplete blocks.";

            if (message.Contains("duplicate key"))
                return "You may have defined the same key twice in the same block — YAML requires keys to be unique.";

            if (message.Contains("found character that cannot start any token"))
                return "There's probably an illegal character or wrong symbol — double-check for stray tabs or weird characters.";

            if (message.Contains("found unexpected ':'"))
                return "There might be a colon `:` in a value that should be quoted — try wrapping the value in quotes.";

            if (message.Contains("anchor") && message.Contains("not defined"))
                return "You're referencing an anchor (&value or *value) that hasn't been defined.";

            if (message.Contains("alias") && message.Contains("not found"))
                return "YAML alias (*) points to something that doesn't exist — check spelling or anchor placement.";

            if (message.Contains("cannot convert") && message.Contains("to"))
                return "A value might be of the wrong type — make sure it's in the correct format (e.g., number vs string).";

            if (message.Contains("sequence entries are not allowed here"))
                return "You're probably using a list (`- item`) in an invalid place — check indentation and nesting.";

            if (message.Contains("unexpected key") || message.Contains("unexpected property"))
                return "This key may be misplaced or invalid — double-check your schema or property names.";

            return "Check your YAML syntax near this location. Be sure indentation, colons, and types are correct.";
        }
    }
}
