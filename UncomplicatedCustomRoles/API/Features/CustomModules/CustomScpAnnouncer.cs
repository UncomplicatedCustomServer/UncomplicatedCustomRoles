using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class CustomScpAnnouncer : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "name"
        };

        internal string RoleName => Args.TryGetValue("name", out string name) ? name : "SCP 404";
    }
}
