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

        [Description("Ignore spawns that are not included in waves and initial spawn? So when you do a forcelass an UCR role won't spawn in any case")]
        public bool AllowOnlyNaturalSpawns { get; set; } = true;

        [Description("Do enable the basic UCR logs?")]
        public bool EnableBasicLogs { get; set; } = true;

        [Description("A list of custom roles")]
        public List<CustomRole> CustomRoles { get; set; } = new()
        {
            new()
        };

        public Dictionary<int, HiddenRoleInformation> HiddenRolesId { get; set; } = new() 
        {
            { 1, new HiddenRoleInformation() }
        };
    }

    public class HiddenRoleInformation
    {
        [Description("If false, any staff with access to RemoteAdmin see the player role.")]
        public bool OnlyVisibleOnOverwatch { get; set; } = false;

        [Description("Empty to get the current display role.")]
        public string RoleNameWhenHidden { get; set; } = "";

    }
}