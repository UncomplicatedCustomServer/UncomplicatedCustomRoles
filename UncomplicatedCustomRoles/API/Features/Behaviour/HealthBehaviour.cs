using PlayerStatsSystem;
using PluginAPI.Core;
using UncomplicatedCustomRoles.API.Helpers;

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
            HealthHelper health = player.ReferenceHub.playerStats.GetModule<HealthStat>() as HealthHelper;
            health._MaxValue = Maximum;

            if (HumeShield > 0)
                player.ReferenceHub.playerStats.GetModule<HumeShieldStat>().CurValue = HumeShield;
        }
    }
}
