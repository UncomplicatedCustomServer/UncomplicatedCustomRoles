using Discord;
using Exiled.API.Features.Doors;
using Newtonsoft.Json;
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
