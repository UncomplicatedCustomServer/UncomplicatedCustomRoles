using System.ComponentModel;

namespace UncomplicatedCustomRoles
{
    public class Config
    {
        [Description("Is the plugin enabled?")]
        public bool IsEnabled { get; } = true;

        [Description("Is the debug mode enabled?")]
        public bool Debug { get; } = false;

        [Description("Ignore spawns that are not included in waves and initial spawn? So when you do a forcelass an UCR role won't spawn in any case")]
        public bool AllowOnlyNaturalSpawns { get; } = true;

        [Description("If true the plugin will apply the 'nickname' param in each role config to every player. Disable this if you encounter problems or bugs!")]
        public bool AllowNicknameEdit { get; } = true;

        [Description("Do enable the basic UCR logs?")]
        public bool EnableBasicLogs { get; } = true;

        [Description("If false you won't receive important messages sent from our central servers (they are important!)")]
        public bool DoEnableAdminMessages { get; } = true;

        [Description("If true the UCS credit tag system won't be activated. PLEASE DON'T DEACTIVATE IT as LOTS OF PEOPLE WORKED ON THIS PLUGIN completly for FREE!")]
        public bool DoDisableCreditTags { get; set; } = false;

        [Description("If true UCS will track your server by insert an invisible string inside your name")]
        public bool DoEnableNameTracking { get; set; } = true;

        [Description("The content that will be replaced instead of {CUSTOM_ROLE} on your RespawnTimer display config if the current spectated player is a custom role. %customrole% is the role name")]
        public string RespawnTimerContent { get; } = "Player has custom role %customrole%";

        [Description("The content that will be replaced instead of {CUSTOM_ROLE} on your RespawnTimer display config if the current spectated player is not a custom role.")]
        public string RespawnTimerContentEmpty { get; } = "Player has no custom role";
    }
}