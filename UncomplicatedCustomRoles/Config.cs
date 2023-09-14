using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Interfaces;
using MapGeneration;
using System;
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
        [Description("A list of custom roles")]
        public List<CustomRole> CustomRoles { get; set; } = new()
        {
            new CustomRole()
        };
    }
}
