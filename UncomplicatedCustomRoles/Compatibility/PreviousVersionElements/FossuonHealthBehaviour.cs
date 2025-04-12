/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

namespace UncomplicatedCustomRoles.Compatibility.PreviousVersionElements
{
    public class FossuonHealthBehaviour
    {
        public int Amount { get; set; } = 100;

        public int Maximum { get; set; } = 100;

        public int HumeShield { get; set; } = 0;

        public float HumeShieldRegenerationAmount { get; set; } = 2;

        public float HumeShieldRegenerationDelay { get; set; } = 7.5f;
    }
}
