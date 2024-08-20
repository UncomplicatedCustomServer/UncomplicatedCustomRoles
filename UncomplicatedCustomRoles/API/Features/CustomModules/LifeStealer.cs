using UncomplicatedCustomRoles.API.Enums;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    internal class LifeStealer : CustomModule
    {
        public new static CustomFlags Flag => CustomFlags.LifeStealer;

        public float Amount { get; set; }

        public LifeStealer(SummonedCustomRole role) : base(role)
        { }

        public override void Execute()
        {
            if (Amount > 0 && HasInstance)
                Instance.Player.Heal(Amount);
            LogManager.Debug($"{Amount} - {HasInstance} - {Instance?.Id}");
        }
    }
}
