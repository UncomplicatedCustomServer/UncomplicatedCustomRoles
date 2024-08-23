using Exiled.API.Features;
using Exiled.API.Features.Roles;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
    public class StaminaBehaviour
    {
        /// <summary>
        /// Gets or sets the regeneration multiplier
        /// </summary>
        public float RegenMultiplier { get; set; } = 1;

        /// <summary>
        /// Gets or sets the usage multiplier
        /// </summary>
        public float UsageMultiplier { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether the stamina should be infinite or not
        /// </summary>
        public bool Infinite { get; set; } = false;

        /// <summary>
        /// Apply the current instance of <see cref="StaminaBehaviour"/> to the given <see cref="Player"/>
        /// </summary>
        /// <param name="player"></param>
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
