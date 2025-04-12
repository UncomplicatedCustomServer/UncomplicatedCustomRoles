using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.Compatibility.PreviousVersionRoles
{
    interface IPreviousVersionRole
    {
        public CustomRole ToCustomRole();
    }
}
