using Exiled.API.Features.Pickups;
using System.Linq;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class DropNothingOnDeath : CustomModule
    {
        public override void OnRemoved()
        {
            foreach (Pickup pickup in Pickup.List.Where(p => Vector3.Distance(p.Position, CustomRole.Player.Position) < 1f && p.PreviousOwner.Id == CustomRole.Player.Id))
                pickup.Destroy();
        }
    }
}
