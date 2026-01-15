/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Text.Json.Serialization;

namespace UncomplicatedCustomRoles.Manager.NET
{
#nullable enable

    internal class VersionInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("source")]
        public string Source { get; }

        [JsonPropertyName("source_link")]
        public string? SourceLink { get; }

        [JsonPropertyName("custom_name")]
        public string? CustomName { get; }

        [JsonPropertyName("pre_release")]
        public bool PreRelease { get; }

        [JsonPropertyName("force_debug")]
        public bool ForceDebug { get; }

        [JsonPropertyName("message")]
        public string Message { get; }

        [JsonPropertyName("recall")]
        public bool Recall { get; }

        [JsonPropertyName("recall_target")]
        public string? RecallTarget { get; }

        [JsonPropertyName("recall_reason")]
        public string? RecallReason { get; }

        [JsonPropertyName("recall_important")]
        public bool? RecallImportant { get; }

        [JsonPropertyName("hash")]
        public string Hash { get; }

        [JsonConstructor]
        public VersionInfo(string name, string source, string? sourceLink, string? customName, bool preRelease, bool forceDebug, string message, bool recall, string? recallTarget, string? recallReason, bool? recallImportant, string hash)
        {
            Name = name;
            Source = source;
            SourceLink = sourceLink;
            CustomName = customName;
            PreRelease = preRelease;
            ForceDebug = forceDebug;
            Message = message;
            Recall = recall;
            RecallTarget = recallTarget;
            RecallReason = recallReason;
            RecallImportant = recallImportant;
            Hash = hash;
        }
    }
}
