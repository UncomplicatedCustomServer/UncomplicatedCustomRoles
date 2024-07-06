using Placeholders.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using Exiled.API.Features;

namespace UncomplicatedCustomRoles.Placeholders.Placeholders
{
    public class RolePlaceholders : IPlaceholder, IPlaceholderPlayerHook
    {
        public string Name => "Role";

        public string Identifier => "ucr";

        public string Author => "SpGerg & FoxWorn";

        public string Description => "Gives all info about role";

        public string OnRequest(string uuid, string identifier)
        {
            var player = Player.Get(uuid);

            if (!player.TryGetCustomRole(out var role))
            {
                return string.Empty;
            }

            switch (identifier)
            {
                case "role_name":
                    return role.Name;
                case "role_model":
                    return role.Role.ToString();
                case "role_id":
                    return role.Id.ToString();
                case "role_description":
                    return role.SpawnBroadcast;
                case "role_max_health":
                    return role.Health.ToString();
                case "role_badge":
                    return role.BadgeName;
                case "role_badge_color":
                    return role.BadgeColor;
            }

            return string.Empty;
        }
    }
}
