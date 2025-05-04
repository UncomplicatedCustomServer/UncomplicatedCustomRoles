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

namespace UncomplicatedCustomRoles.API.Interfaces
{
    internal interface IUCRCommand
    {
        public string Name { get; }

        public string Description { get; }

        public string RequiredPermission { get; }

        public bool Executor(List<string> arguments, ICommandSender sender, out string response);
    }
}
