using System;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    class CustomScpAnnouncer : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "name"
        };
    }
}
