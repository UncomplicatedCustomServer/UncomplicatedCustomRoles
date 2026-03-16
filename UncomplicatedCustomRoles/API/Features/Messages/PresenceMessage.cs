using LabApi.Features.Wrappers;
using System.Text.Json.Serialization;

namespace UncomplicatedCustomRoles.API.Features.Messages
{
    internal class PresenceMessage
    {
        [JsonPropertyName("server_id")]
        public int Port { get; set; } = Server.Port;

        [JsonPropertyName("player_count")]
        public int PlayerCount { get; set; } = Server.PlayerCount;

        [JsonPropertyName("max_players")]
        public int MaxPlayers { get; set; } = Server.MaxPlayers;

        [JsonPropertyName("name")]
        public string Name { get; set; } = Server.ServerListName;

        [JsonPropertyName("max_tps")]
        public int MaxTps { get; set; } = Server.MaxTps;

        [JsonPropertyName("tps")]
        public double Tps { get; set; } = Server.Tps;

        [JsonPropertyName("plugin")]
        public string PluginName => "UCR";

        [JsonPropertyName("version")]
        public string Version { get; set; } = Plugin.Instance.Version.ToString(4);



        public PresenceMessage()
        { }
    }
}
