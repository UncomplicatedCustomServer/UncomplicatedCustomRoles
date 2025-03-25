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

        [Description("If true the plugin will apply the 'nickname' param in each role config to every player. Disable this if you encounter problems or bugs!")]
        public bool AllowNicknameEdit { get; set; } = true;

        [Description("If true UCR will override the name given by RPNames")]
        public bool OverrideRpNames { get; set; } = true;

        [Description("Do enable the basic UCR logs?")]
        public bool EnableBasicLogs { get; set; } = true;

        [Description("If true the UCS credit tag system won't be activated. PLEASE DON'T DEACTIVATE IT as LOTS OF PEOPLE WORKED ON THIS PLUGIN completly for FREE!")]
        public bool EnableCreditTags { get; set; } = true;

        [Description("Whether the NPCs can naturally spawn custom roles")]
        public bool IgnoreNpcs { get; set; } = true;

        [Description("Whether you want your spawnpoints to be hosted inside our central server or locally in the configs folder")]
        public bool LocalSpawnPoints { get; set; } = false;

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