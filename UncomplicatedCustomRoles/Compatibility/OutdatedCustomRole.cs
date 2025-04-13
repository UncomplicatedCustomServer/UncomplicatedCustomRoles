/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Compatibility
{
    public class OutdatedCustomRole
    {
        public ICustomRole CustomRole { get; }

        public Version Version { get; }

        public string Path { get; }

        internal OutdatedCustomRole(ICustomRole customRole, Version version, string path)
        {
            CustomRole = customRole;
            Version = version;
            Path = path;
        }
    }
}
