using PluginAPI.Core;

namespace UncomplicatedCustomRoles.Events.Interfaces
{
    public interface IPlayerEvent
    {
        public abstract ReferenceHub Hub { get; }
        public abstract Player Player { get; }
    }
}
