using PlayerRoles;
using UncomplicatedCustomRoles.Events.Interfaces;

namespace UncomplicatedCustomRoles.Events.Args
{
    internal class ChangingRoleEventArgs : EventArgs, IPlayerEvent, IDeniableEvent
    {
        public ReferenceHub Hub { get; }

        public RoleTypeId OldRole { get; }

        public RoleTypeId NewRole { get; set; }

        public RoleChangeReason RoleChangeReason { get; set; }

        public RoleSpawnFlags RoleSpawnFlags { get; set; }

        public bool IsAllowed { get; set; } = true;

        public ChangingRoleEventArgs(ReferenceHub hub, RoleTypeId oldRole, RoleTypeId newRole, RoleChangeReason roleChangeReason, RoleSpawnFlags roleSpawnFlags)
        {
            Hub = hub;
            OldRole = oldRole;
            NewRole = newRole;
            RoleChangeReason = roleChangeReason;
            RoleSpawnFlags = roleSpawnFlags;
        }
    }
}
