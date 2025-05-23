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
using System.Linq;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class DropNothingOnDeath : CustomModule
    {
        public override void OnRemoved()
        {
            foreach (Pickup pickup in Pickup.List.Where(p => Vector3.Distance(p.Position, CustomRole.Player.Position) < 1f && p.LastOwner.PlayerId == CustomRole.Player.PlayerId))
                pickup.Destroy();
        }
    }
}
