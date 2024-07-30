using PluginAPI.Core;

namespace UncomplicatedCustomRoles.Events.Interfaces
{
    public interface IAttackerEvent
    {
        public abstract ReferenceHub AttackerHub { get; }
        public abstract Player Attacker { get; }
    }
}
