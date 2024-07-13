using Exiled.API.Features;
using Exiled.API.Features.Roles;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
    public class StaminaBehaviour
    {
        public float RegenMultiplier { get; set; } = 1;

        public float UsageMultiplier { get; set; } = 1;

        public bool Infinite { get; set; } = false;

        public void Apply(Player player)
        {
            if (player.Role is FpcRole Fpc)
            {
                Fpc.StaminaRegenMultiplier = RegenMultiplier;
                Fpc.StaminaUsageMultiplier = UsageMultiplier;
            }

            player.IsUsingStamina = !Infinite;
        }
    }
}
