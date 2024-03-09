using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using UncomplicatedCustomRoles.Structures;

namespace UncomplicatedCustomRoles.Elements
{
    public class CustomRoleEvent : ICustomRoleEvent
    {
        public Player Player { get; }
        public ICustomRole Role { get; }
        public UCREvents EventType { get; }
        public IPlayerEvent Event { get; }
        public bool IsAllowed { get; set; }
        public CustomRoleEvent(ICustomRole role, UCREvents eventType, IPlayerEvent @event)
        {
            Player = @event.Player;
            Role = role;
            EventType = eventType;
            Event = @event;
            IsAllowed = true;
        }
    }
}
