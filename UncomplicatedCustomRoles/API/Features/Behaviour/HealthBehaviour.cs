using Exiled.API.Features;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
    public class HealthBehaviour
    {
        /// <summary>
        /// Gets or sets the amout of health that has to be given to the player
        /// </summary>
        public int Amount { get; set; } = 100;

        /// <summary>
        /// Gets or sets the maximum amout of health
        /// </summary>
        public int Maximum { get; set; } = 100;

        /// <summary>
        /// Gets or sets the hume shield amout of the 
        /// </summary>
        public int HumeShield { get; set; } = 0;

        /// <summary>
        /// Gets or sets the speed of the regeneration of the hume shield in units/second
        /// </summary>
        public float HumeShieldRegenerationAmount { get; set; } = 2;

        /// <summary>
        /// Gets or sets the time that the player has to be untouched (not damaged) in order to regen the hume shield (in seconds)
        /// </summary>
        public float HumeShieldRegenerationDelay { get; set; } = 7.5f;

        /// <summary>
        /// Apply the current instance of <see cref="HealthBehaviour"/> to the given <see cref="Player"/>
        /// </summary>
        /// <param name="player"></param>
        public void Apply(Player player)
        {
            player.Health = Amount;
            player.MaxHealth = Maximum;

            if (HumeShield > 0)
                player.HumeShield = HumeShield;
        }
    }
}
