using Exiled.Permissions.Extensions;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class CustomPermissions : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "permissions"
        };

        internal string[] Permissions => Args.TryGetValue("permissions", out string permissions) ? permissions.Replace(" ", string.Empty).Split(',') : new string[] { };
    }
}
