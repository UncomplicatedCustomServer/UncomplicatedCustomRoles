using PlayerRoles;
using PlayerStatsSystem;
using System;
using UncomplicatedCustomRoles.API.Enums;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class CustomScpAnnouncer : CustomModule
    {
        public new static CustomFlags Flag => CustomFlags.CustomScpAnnouncer;

        public DamageHandlerBase DamageHandler { get; private set; } = null;

        public bool IsAvailable { get; private set; }

        public CustomScpAnnouncer(SummonedCustomRole role) : base(role) { }

        public void Awake(DamageHandlerBase damageHandlerBase)
        {
            if ((Instance.Role?.Team ?? Instance.Role.Role.GetTeam()) is Team.SCPs)
            {
                IsAvailable = true;
                DamageHandler = damageHandlerBase;
            }
        }

        public override void Execute()
        {
            IsAvailable = false;
            DamageHandler = null;
        }
    }
}
