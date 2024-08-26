using UncomplicatedCustomRoles.API.Enums;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    internal class SilentAnnouncer : CustomModule
    {
        public new static CustomFlags Flag => CustomFlags.SilentAnnouncer;
    }
}
