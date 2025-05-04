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
    
namespace UncomplicatedCustomRoles.API.Interfaces
{
    public interface IEffect
    {
        public abstract EffectType EffectType { get; set; }

        public abstract float Duration { get; set; }

        public abstract byte Intensity { get; set; }

        public abstract bool Removable { get; set; }
    }
}