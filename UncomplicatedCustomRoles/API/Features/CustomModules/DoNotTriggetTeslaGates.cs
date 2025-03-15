using Exiled.Events.EventArgs.Interfaces;
using System;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    class DoNotTriggetTeslaGates : CustomModule
    {
        public override List<string> TriggerOnEvents => new()
        {
            "TriggeringTesla"
        };

        public override bool OnEvent(string name, IPlayerEvent ev) => false;
    }
}
