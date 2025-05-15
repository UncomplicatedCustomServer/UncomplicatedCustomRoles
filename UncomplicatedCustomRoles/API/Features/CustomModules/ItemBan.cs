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
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class ItemBan : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "item_type"
        };

        public ItemType? Type => Args.TryGetValue("item_type", out string itemType) && Enum.TryParse(itemType, out ItemType type) ? type : null;

        public static bool ValidatePickup(SummonedCustomRole role, Pickup pickup)
        {
            List<ItemType> notAllowedTypes = new();

            foreach (ItemBan itemBan in role.GetModules<ItemBan>())
                if (itemBan.Type is ItemType type)
                    notAllowedTypes.Add(type);

            return !notAllowedTypes.Contains(pickup.Type);
        }
    }
}
