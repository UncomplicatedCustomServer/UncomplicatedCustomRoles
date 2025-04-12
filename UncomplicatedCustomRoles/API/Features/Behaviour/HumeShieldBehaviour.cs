using Exiled.API.Features;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
    public class HumeShieldBehaviour
    {
        /// <summary>
        /// Gets or sets the hume shield amount
        /// </summary>
        public int Amount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum hume shield amount
        /// </summary>
        public int Maximum { get; set; } = 0;

        /// <summary>
        /// Gets or sets the speed of the regeneration of the hume shield in units/second
        /// </summary>
        public float HumeShieldRegenerationAmount { get; set; } = 2;

        /// <summary>
        /// Gets or sets the time that the player has to be untouched (not damaged) in order to regen the hume shield (in seconds)
        /// </summary>
        public float HumeShieldRegenerationDelay { get; set; } = 7.5f;

        /// <summary>
        /// Apply the current instance of <see cref="HumeShieldBehaviour"/> to the given <see cref="Player"/>
        /// </summary>
        /// <param name="player"></param>
        public void Apply(Player player)
        {
            if (Amount > 0)
            {
                player.HumeShield = Amount;
                player.MaxHumeShield = Maximum;
            }
        }
    }
}
