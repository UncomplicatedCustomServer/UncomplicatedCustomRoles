using Exiled.Events.EventArgs.Interfaces;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class SilentWalker : CustomModule
    {
        public override List<string> TriggerOnEvents => new()
        {
            "MakingNoise"
        };

        public override bool OnEvent(string name, IPlayerEvent ev) => false;
    }
}
