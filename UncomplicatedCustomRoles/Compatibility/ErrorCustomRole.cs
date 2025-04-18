/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using PlayerRoles;
using System;

namespace UncomplicatedCustomRoles.Compatibility
{
    public class ErrorCustomRole
    {
        /// <summary>
        /// Gets the path of the CustomRole with the error
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the content of the CustomRole with the error
        /// </summary>
        public string[] Content { get; }

        /// <summary>
        /// Gets the error
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets the error message.<br></br>
        /// Auto-generated if none is put
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the CustomRole Id
        /// </summary>
        public string Id => CompatibilityManager.GetRoleFileElement(Content, "id:");

        /// <summary>
        /// Gets the CustomRole Name
        /// </summary>
        public string Name => CompatibilityManager.GetRoleFileElement(Content, "name:", false).Replace("'", string.Empty).Replace("\"", string.Empty);

        /// <summary>
        /// Gets the CustomRole raw Role
        /// </summary>
        public string RawRole => CompatibilityManager.GetRoleFileElement(Content, "role:");

        /// <summary>
        /// Gets the CustomRole Role as a <see cref="RoleTypeId"/>.<br></br>
        /// <see cref="RoleTypeId.None"/> if not found
        /// </summary>
        public RoleTypeId Role => Enum.TryParse(RawRole, out RoleTypeId role) ? role : RoleTypeId.None;

        internal ErrorCustomRole(string path, string[] content, Exception exception, string message = null)
        {
            Path = path;
            Content = content;
            Exception = exception;
            Message = message ?? CompatibilityManager.HandleErrorString(exception, true);
        }
    }
}
