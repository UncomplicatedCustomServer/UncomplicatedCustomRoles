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

        internal List<CandyKindID> Kinds => Args.TryGetValue("candies", out object kinds) ? kinds as List<CandyKindID> : new();

        public override void OnAdded()
        {
            foreach (CandyKindID kind in Kinds)
                CustomRole.Player.GiveCandy(kind, ItemAddReason.AdminCommand);
        }
    }
}