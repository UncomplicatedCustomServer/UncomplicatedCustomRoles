﻿/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json;

namespace UncomplicatedCustomRoles.Manager.NET
{
#nullable enable

    internal class VersionInfo
    {
        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("source")]
        public string Source { get; }

        [JsonProperty("source_link")]
        public string? SourceLink { get; }

        [JsonProperty("custom_name")]
        public string? CustomName { get; }

        [JsonProperty("pre_release")]
        public bool PreRelease { get; }

        [JsonProperty("force_debug")]
        public bool ForceDebug { get; }

        [JsonProperty("message")]
        public string Message { get; }

        [JsonProperty("recall")]
        public bool Recall { get; }

        [JsonProperty("recall_target")]
        public string? RecallTarget { get; }

        [JsonProperty("recall_reason")]
        public string? RecallReason { get; }

        [JsonProperty("recall_important")]
        public bool? RecallImportant { get; }

        [JsonProperty("hash")]
        public string Hash { get; }

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
