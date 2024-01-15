using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

using UncomplicatedCustomRoles.Elements;

namespace UncomplicatedCustomRoles
{
    internal class Config : IConfig
    {
        [Description("Is the plugin enabled?")]
        public bool IsEnabled { get; set; } = true;
        [Description("Is the debug mode enabled?")]
        public bool Debug { get; set; } = false;
        [Description("Enable the HTTP request for the presence update? Please, this thing is important for us (UCS)!")]
        public bool EnableHttp { get; set; } = true;
        [Description("A list of custom roles")]
        public List<CustomRole> CustomRoles { get; set; } = new()
        {
            new CustomRole()
        };
    }
}