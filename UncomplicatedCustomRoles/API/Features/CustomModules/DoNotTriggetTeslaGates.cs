﻿using Exiled.Events.EventArgs.Interfaces;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class DoNotTriggetTeslaGates : CustomModule
    {
        public override List<string> TriggerOnEvents => new()
        {
            "TriggeringTesla"
        };

        public override bool OnEvent(string name, IPlayerEvent ev) => false;
    }
}
