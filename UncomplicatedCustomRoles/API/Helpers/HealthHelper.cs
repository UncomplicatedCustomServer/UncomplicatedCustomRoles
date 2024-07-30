using PlayerStatsSystem;

namespace UncomplicatedCustomRoles.API.Helpers
{
#pragma warning disable IDE1006 // Stili di denominazione
    public class HealthHelper : HealthStat
    {
        public override float MaxValue => _MaxValue == default ? base.MaxValue : _MaxValue;

        public float _MaxValue { get; set; }
    }
}
