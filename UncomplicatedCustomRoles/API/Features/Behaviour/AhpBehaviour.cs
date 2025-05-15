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
    public class AhpBehaviour
    {
        /// <summary>
        /// Gets or sets the amout of AHP
        /// </summary>
        public float Amount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the limit of AHP
        /// </summary>
        public float Limit { get; set; } = 75;

        /// <summary>
        /// Gets or sets the decay speed of AHP
        /// </summary>
        public float Decay { get; set; } = 1.2f;

        /// <summary>
        /// Gets or sets the efficacy of AHP
        /// </summary>
        public float Efficacy { get; set; } = 0.7f;

        /// <summary>
        /// Gets or sets for how long will the AHP remain static (do not decay)
        /// </summary>
        public float Sustain { get; set; } = 0f;

        /// <summary>
        /// Gets or sets whether the AHP must be persistant or not
        /// </summary>
        public bool Persistant { get; set; } = false;

        /// <summary>
        /// Apply the current instance of <see cref="AhpBehaviour"/> to the given <see cref="Player"/>
        /// </summary>
        /// <param name="player"></param>
        public void Apply(Player player)
        {
            if (Amount > 0)
                player.CreateAhpProcess(Amount, Limit, Decay, Efficacy, Sustain, Persistant);
        }
    }
}
