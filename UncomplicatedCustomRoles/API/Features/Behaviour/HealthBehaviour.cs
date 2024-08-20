using Exiled.API.Features;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
    public class HealthBehaviour
    {
        public int Amount { get; set; } = 100;

        public int Maximum { get; set; } = 100;

        public int HumeShield { get; set; } = 0;

        public void Apply(Player player)
        {
            player.Health = Amount;
            player.MaxHealth = Maximum;

            if (HumeShield > 0)
                player.HumeShield = HumeShield;
        }
    }
}
