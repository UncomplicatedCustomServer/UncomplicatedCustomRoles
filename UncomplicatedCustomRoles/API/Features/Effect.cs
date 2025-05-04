/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.API.Enums;
using UncomplicatedCustomRoles.API.Interfaces;

namespace UncomplicatedCustomRoles.Manager
{
    public class Effect : IEffect
    {
        /// <summary>
        /// Gets or sets the <see cref="EffectType"/> of the effect
        /// </summary>
        public EffectType EffectType { get; set; } = EffectType.MovementBoost;

        /// <summary>
        /// Gets or sets the duration of the effect
        /// </summary>
        public float Duration { get; set; } = -1;

        /// <summary>
        /// Gets or sets the intensity of the effect
        /// </summary>
        public byte Intensity { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether the effect can be removed by using SCP-500
        /// </summary>
        public bool Removable { get; set; } = false;
    }
}