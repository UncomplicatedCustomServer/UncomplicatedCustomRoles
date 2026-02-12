/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using InventorySystem.Items.Usables.Scp330;
using System.Collections.Generic;
using InventorySystem.Items;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class FullCandyBag : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "candies"
        };

        internal List<string> Kinds         
        {
            get
            {
                if (!Args.TryGetValue("candies", out object items) || items == null)
                    return new List<string>();
                
                return ConvertToList(items);
            }
        }

        public override void OnAdded()
        {
            foreach (string kind in Kinds)
            {
                if (Enum.TryParse(kind, out CandyKindID candyKind))
                    CustomRole.Player.GiveCandy(candyKind, ItemAddReason.AdminCommand);
            }
        }
    }
}