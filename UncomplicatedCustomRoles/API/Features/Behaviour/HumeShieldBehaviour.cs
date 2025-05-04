/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.API.Features;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
    public class HumeShieldBehaviour
    {
        /// <summary>
        /// Gets or sets the hume shield amount
        /// </summary>
        public int Amount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum hume shield amount
        /// </summary>
        public int Maximum { get; set; } = 0;

        /// <summary>
        /// Gets or sets the speed of the regeneration of the hume shield in units/second
        /// </summary>
        public float RegenerationAmount { get; set; } = 2;

        /// <summary>
        /// Gets or sets the time that the player has to be untouched (not damaged) in order to regen the hume shield (in seconds)
        /// </summary>
        public float RegenerationDelay { get; set; } = 7.5f;

        /// <summary>
        /// Apply the current instance of <see cref="HumeShieldBehaviour"/> to the given <see cref="Player"/>
        /// </summary>
        /// <param name="player"></param>
        public void Apply(Player player)
        {
            if (Amount > 0)
            {
                player.HumeShield = Amount;
                player.MaxHumeShield = Maximum;
            }
        }
    }
}
