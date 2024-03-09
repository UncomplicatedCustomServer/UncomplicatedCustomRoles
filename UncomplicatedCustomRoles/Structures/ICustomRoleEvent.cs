using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;

namespace UncomplicatedCustomRoles.Structures
{
    public interface ICustomRoleEvent
    {
        public abstract Player Player { get; }
        public abstract ICustomRole Role {  get; }
        public abstract UCREvents EventType { get; }
        public abstract IPlayerEvent Event { get; }
        public bool IsAllowed { get; set; }
    }
}
