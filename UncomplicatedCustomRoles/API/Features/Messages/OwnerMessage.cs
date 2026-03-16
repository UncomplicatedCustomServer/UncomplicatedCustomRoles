using LabApi.Features.Wrappers;
using System.Text.Json.Serialization;

namespace UncomplicatedCustomRoles.API.Features.Messages
{
    internal class OwnerMessage
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("discord_id")]
        public string DiscordId { get; set; }

        public OwnerMessage(Player player, string discordId)
        {
            UserId = player.UserId;
            DiscordId = discordId;
        }
    }
}
