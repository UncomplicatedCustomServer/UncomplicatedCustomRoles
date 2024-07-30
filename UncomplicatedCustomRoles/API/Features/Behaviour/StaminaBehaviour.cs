using PluginAPI.Core;

namespace UncomplicatedCustomRoles.API.Features.Behaviour
{
    public class StaminaBehaviour
    {
        public float RegenMultiplier { get; set; } = 1;

        public float UsageMultiplier { get; set; } = 1;

        public bool Infinite { get; set; } = false;

        public void Apply(Player _) { }
    }
}
