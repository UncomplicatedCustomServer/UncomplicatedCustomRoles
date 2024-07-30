using PlayerStatsSystem;
using PluginAPI.Core;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
    public class AhpBehaviour
    {
        public float Amount { get; set; } = 0;

        public float Limit { get; set; } = 75;

        public float Decay { get; set; } = 1.2f;

        public float Efficacy { get; set; } = 0.7f;

        public float Sustain { get; set; } = 0f;

        public bool Persistant { get; set; } = false;

        public void Apply(Player player)
        {
            if (Amount > 0)
                player.ReferenceHub.playerStats.GetModule<AhpStat>().ServerAddProcess(Amount, Limit, Decay, Efficacy, Sustain, Persistant);
        }
    }
}
