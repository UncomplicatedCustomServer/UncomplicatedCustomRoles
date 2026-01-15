/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Discord;
using System.Text.Json.Serialization;
using System;

namespace UncomplicatedCustomRoles.API.Features
{
    internal class LogEntry
    {
        /// <summary>
        /// Gets the time in unix milliseconds of the message
        /// </summary>
        public long Time { get; }

        /// <summary>
        /// Gets the <see cref="Discord.LogLevel"/> or a custom LogLevel of the message
        /// </summary>
        public string Level { get; }

        /// <summary>
        /// Gets the message of the log
        /// </summary>
        public string Content { get; }

#nullable enable
        /// <summary>
        /// Gets the custom error code of the message - can be null!
        /// </summary>
        public string? Error { get; }
#nullable disable

        /// <summary>
        /// Gets the instance of the Error as string
        /// </summary>
        public string PublicError => Error is null ? string.Empty : $"{Error} ";

        [JsonIgnore]
        public DateTimeOffset DateTimeOffset => DateTimeOffset.FromUnixTimeMilliseconds(Time);

        [JsonConstructor]
        public LogEntry(long time, string level, string content, string error = null)
        {
            Time = time;
            Level = level;
            Content = content;
            Error = error;
        }

        public LogEntry(long time, LogLevel level, string content, string error = null) : this(time, level.ToString(), content, error) { }

        public override string ToString() => $"[{DateTimeOffset.Year}-{DateTimeOffset.Month}-{DateTimeOffset.Day} {DateTimeOffset.Hour}:{DateTimeOffset.Minute}:{DateTimeOffset.Second} {DateTimeOffset.Offset}]  [{Level}]  [UncomplicatedCustomRoles] {PublicError}{Content}";
    }
}
