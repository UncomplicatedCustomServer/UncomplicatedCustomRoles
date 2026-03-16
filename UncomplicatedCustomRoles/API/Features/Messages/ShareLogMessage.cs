using LabApi.Features;
using System.Text.Json.Serialization;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features.Messages
{
    internal class ShareLogMessage
    {
        [JsonPropertyName("labapi_version")]
        public string LabAPIVersion { get; set; } = LabApiProperties.CompiledVersion;

        [JsonPropertyName("plugin_version")]
        public string PluginVersion { get; set; } = Plugin.Instance.Version.ToString(4);

        [JsonPropertyName("hash")]
        public string Hash { get; set; } = VersionManager.HashFile(Plugin.Instance.FilePath);

        [JsonPropertyName("message")]
        public string Message { get; set; }

        public ShareLogMessage(string message)
        {
            Message = message;
        }
    }
}
