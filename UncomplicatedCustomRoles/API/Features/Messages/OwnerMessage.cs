using System.Text.Json.Serialization;
using LabApi.Features.Wrappers;

namespace UncomplicatedCustomRoles.API.Features.Messages;

internal class OwnerMessage
{
    public OwnerMessage(Player player, string discordId)
    {
        UserId = player.UserId;
        DiscordId = discordId;
    }

    [JsonPropertyName("user_id")] public string UserId { get; set; }

    [JsonPropertyName("discord_id")] public string DiscordId { get; set; }
}