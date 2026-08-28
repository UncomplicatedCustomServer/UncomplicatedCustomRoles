using System.Text.Json.Serialization;
using LabApi.Features;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features.Messages;

internal class ShareLogMessage(string message)
{
    [JsonPropertyName("labapi_version")] public string LabAPIVersion { get; set; } = LabApiProperties.CompiledVersion;

    [JsonPropertyName("plugin_version")]
    public string PluginVersion { get; set; } = Plugin.Instance.Version.ToString();

    [JsonPropertyName("hash")] public string Hash { get; set; } = VersionManager.HashFile(Plugin.Instance.FilePath);

    [JsonPropertyName("message")] public string Message { get; set; } = message;
}