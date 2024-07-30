using GameCore;
using PlayerRoles;
using PluginAPI.Core;
using Respawning;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Events.Interfaces;
using Log = PluginAPI.Core.Log;

namespace UncomplicatedCustomRoles.Events.Args
{
    public class RespawningTeamEventArgs : EventArgs, IDeniableEvent
    {
        /// <summary>
        /// Gets or sets whether the event is allowed or not
        /// </summary>
        public bool IsAllowed { get; set; } = true;

        /// <summary>
        /// Gets or sets the respawn queue
        /// </summary>
        public List<ReferenceHub> RespawnQueue { get; set; }

        /// <summary>
        /// Gets or sets the role spawn queue
        /// </summary>
        public Queue<RoleTypeId> RoleQueue { get; set; }

        /// <summary>
        /// Gets the max wave size
        /// </summary>
        public int MaxWaveSize { get; }

        /// <summary>
        /// Gets or sets the next known team
        /// </summary>
        public SpawnableTeamType NextKnownTeam { get; set; }

        public RespawningTeamEventArgs(IEnumerable<ReferenceHub> respawnQueue, Queue<RoleTypeId> roleQueue, int maxSize, SpawnableTeamType nextKnownTeam)
        {
            RespawnQueue = respawnQueue.ToList();
            RoleQueue = roleQueue;
            MaxWaveSize = maxSize;
            NextKnownTeam = nextKnownTeam;
            Log.Debug($"RespawningTeam: {MaxWaveSize} and {NextKnownTeam}");
        }
    }
}
