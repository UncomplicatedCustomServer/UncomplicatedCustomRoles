using Exiled.API.Features;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
    public class HealthBehaviour
    {
        public float Amount { get; set; } = 100;

        public float Maximum { get; set; } = 100;

        public float HumeShield { get; set; } = 0;

        public void Apply(Player player)
        {
            player.Health = Amount;
            player.MaxHealth = Maximum;

            if (HumeShield > 0)
                player.HumeShield = HumeShield;
        }
    }
}
