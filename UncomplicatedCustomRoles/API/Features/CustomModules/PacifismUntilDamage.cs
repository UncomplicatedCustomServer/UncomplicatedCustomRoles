using UncomplicatedCustomRoles.API.Enums;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class PacifismUntilDamage : CustomModule
    {
        public new static CustomFlags Flag => CustomFlags.PacifismUntilDamage;

        public bool IsPacifist { get; internal set; }

        public override void Execute() => IsPacifist = false;
    }
}
