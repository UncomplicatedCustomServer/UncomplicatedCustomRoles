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
using PlayerRoles.FirstPersonControl;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
    public class StaminaBehaviour
    {
        /// <summary>
        /// Gets or sets the regeneration multiplier
        /// </summary>
        public float RegenMultiplier { get; set; } = 1;

        /// <summary>
        /// Gets or sets the usage multiplier
        /// </summary>
        public float UsageMultiplier { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether the stamina should be infinite or not
        /// </summary>
        public bool Infinite { get; set; } = false;

        /// <summary>
        /// Apply the current instance of <see cref="StaminaBehaviour"/> to the given <see cref="Player"/>
        /// </summary>
        /// <param name="_"></param>
        public void Apply(Player _)
        { }
    }
}
