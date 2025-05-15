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

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
    public class HealthBehaviour
    {
        /// <summary>
        /// Gets or sets the amout of health that has to be given to the player
        /// </summary>
        public int Amount { get; set; } = 100;

        /// <summary>
        /// Gets or sets the maximum amout of health
        /// </summary>
        public int Maximum { get; set; } = 100;

        /// <summary>
        /// Apply the current instance of <see cref="HealthBehaviour"/> to the given <see cref="Player"/>
        /// </summary>
        /// <param name="player"></param>
        public void Apply(Player player)
        {
            player.Health = Amount;
            player.MaxHealth = Maximum;
        }
    }
}
