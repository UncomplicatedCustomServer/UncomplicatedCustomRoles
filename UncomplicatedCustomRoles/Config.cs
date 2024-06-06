using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

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

        [Description("The content that will be replaced instead of {CUSTOM_ROLE} on your RespawnTimer display config if the current spectated player is a custom role. %customrole% is the role name")]
        public string RespawnTimerContent { get; set; } = "Player has custom role %customrole%";

        [Description("The content that will be replaced instead of {CUSTOM_ROLE} on your RespawnTimer display config if the current spectated player is not a custom role.")]
        public string RespawnTimerContentEmpty { get; set; } = "Player has no custom role";

        [Description("If the role Id is here UCR won't take the role name but the following config")]
        public Dictionary<int, HiddenRoleInformation> HiddenRolesId { get; set; } = new() 
        {
            { 1, new HiddenRoleInformation() }
        };
    }

    public class HiddenRoleInformation
    {
        [Description("This custom role will be visible only for those who are in Overwatch.")]
        public bool OnlyVisibleOnOverwatch { get; set; } = false;

        [Description("Empty to get the current display role.")]
        public string RoleNameWhenHidden { get; set; } = "";
    }
}