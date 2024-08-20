using UncomplicatedCustomRoles.API.Enums;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    internal class NotAffectedByAppearance : CustomModule
    {
        public new static CustomFlags Flag => CustomFlags.NotAffectedByAppearance;
    }
}
