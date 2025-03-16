using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class LifeStealer : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "percentage"
        };

        public int Percentage => Args.TryGetValue("percentage", out string perc) && int.TryParse(perc, out int numPerc) ? numPerc : 0; // NOTE: Percentage MUST be an int so like 75 is 75% (0.75)
    }
}
