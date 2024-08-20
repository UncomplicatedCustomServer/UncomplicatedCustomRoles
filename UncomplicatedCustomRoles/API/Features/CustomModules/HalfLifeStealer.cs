using UncomplicatedCustomRoles.API.Enums;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class HalfLifeStealer : CustomModule
    {
        public new static CustomFlags Flag => CustomFlags.HalfLifeStealer;

        public float Percentage => 0.5f;

        public float Amount { get; set; }

        public HalfLifeStealer(SummonedCustomRole role) : base(role)
        { }

        public override void Execute()
        {
            if (Amount > 0 && HasInstance)
                Instance.Player.Heal(Amount * Percentage);
        }
    }
}
