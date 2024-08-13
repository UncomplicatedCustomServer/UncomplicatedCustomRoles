using PlayerStatsSystem;
using PluginAPI.Core;
using UncomplicatedCustomRoles.API.Helpers;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
    public class HealthBehaviour
    {
        public float Amount { get; set; } = 100;

        public float Maximum { get; set; } = 100;

        public float HumeShield { get; set; } = 0;

        public void Apply(Player player)
        {
            LogManager.Silent("[HP] AM");
            player.Health = Amount;
            LogManager.Silent("[HP] AS1");
            HealthHelper health = player.ReferenceHub.playerStats.GetModule<HealthStat>() as HealthHelper;
            LogManager.Silent("[HP] AS2");
            //health._MaxValue = Maximum;

            if (HumeShield > 0)
            {
                LogManager.Silent("[HP] HS");
                player.ReferenceHub.playerStats.GetModule<HumeShieldStat>().CurValue = HumeShield;
            }
        }
    }
}
