using Exiled.API.Features.Pickups;
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
