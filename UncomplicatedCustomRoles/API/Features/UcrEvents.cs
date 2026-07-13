/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using LabApi.Features.Wrappers;
using System;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.API.Features
{
    /// <summary>
    /// Public static event bus for external plugins/extensions to subscribe to UCR lifecycle events.
    /// This is the primary integration point for addons like EndConditionsExtension.
    /// </summary>
    public static class UcrEvents
    {
        /// <summary>
        /// Data passed along with every UCR event.
        /// </summary>
        public class CustomRoleEventData : EventArgs
        {
            /// <summary>
            /// Gets the <see cref="LabApi.Features.Wrappers.Player"/> involved in the event.
            /// </summary>
            public Player Player { get; }

            /// <summary>
            /// Gets the <see cref="ICustomRole"/> definition of the role.
            /// </summary>
            public ICustomRole Role { get; }

            /// <summary>
            /// Gets the <see cref="SummonedCustomRole"/> instance, if available.
            /// May be null in removal events if the instance was already destroyed.
            /// </summary>
            public SummonedCustomRole SummonedRole { get; }

            public CustomRoleEventData(Player player, ICustomRole role, SummonedCustomRole summonedRole)
            {
                Player = player;
                Role = role;
                SummonedRole = summonedRole;
            }
        }

        /// <summary>
        /// Data passed when a custom role player escapes.
        /// </summary>
        public class CustomRoleEscapeEventData : CustomRoleEventData
        {
            /// <summary>
            /// Gets whether the player was cuffed/disarmed when escaping.
            /// </summary>
            public bool IsCuffed { get; }

            /// <summary>
            /// Gets the new role the player is being assigned to after escaping.
            /// May be null if the player receives a natural role change.
            /// </summary>
            public ICustomRole NewRole { get; }

            /// <summary>
            /// Gets the <see cref="ICustomRole.CustomTeamId"/> of the escaping role, for convenience.
            /// </summary>
            public string CustomTeamId => Role?.CustomTeamId;

            public CustomRoleEscapeEventData(Player player, ICustomRole role, SummonedCustomRole summonedRole, bool isCuffed, ICustomRole newRole = null)
                : base(player, role, summonedRole)
            {
                IsCuffed = isCuffed;
                NewRole = newRole;
            }
        }

        /// <summary>
        /// Fired after a player has been assigned a custom role and fully initialized.
        /// </summary>
        public static event EventHandler<CustomRoleEventData> CustomRoleAssigned;

        /// <summary>
        /// Fired just before a player's custom role is destroyed/removed.
        /// </summary>
        public static event EventHandler<CustomRoleEventData> CustomRoleRemoved;

        /// <summary>
        /// Fired when a player with a custom role successfully escapes.
        /// </summary>
        public static event EventHandler<CustomRoleEscapeEventData> CustomRoleEscaped;

        internal static void RaiseCustomRoleAssigned(Player player, ICustomRole role, SummonedCustomRole summonedRole)
        {
            CustomRoleAssigned?.Invoke(null, new CustomRoleEventData(player, role, summonedRole));
        }

        internal static void RaiseCustomRoleRemoved(Player player, ICustomRole role, SummonedCustomRole summonedRole)
        {
            CustomRoleRemoved?.Invoke(null, new CustomRoleEventData(player, role, summonedRole));
        }

        internal static void RaiseCustomRoleEscaped(Player player, ICustomRole role, SummonedCustomRole summonedRole, bool isCuffed, ICustomRole newRole = null)
        {
            CustomRoleEscaped?.Invoke(null, new CustomRoleEscapeEventData(player, role, summonedRole, isCuffed, newRole));
        }
    }
}
