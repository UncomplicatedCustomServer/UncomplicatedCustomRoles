using Exiled.API.Features.Pickups;
using MEC;
using System;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class DropItemOnDeath : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "item"
        };

        public ItemType? Item => StringArgs.TryGetValue("item", out string rawItem) && Enum.TryParse(rawItem, out ItemType item) && item is not ItemType.None ? item : null;

        public override void OnRemoved()
        {
            if (Item is ItemType item)
                Timing.CallDelayed(0.5f, () => Pickup.CreateAndSpawn(item, CustomRole.Player.Position));
        }
    }
}
