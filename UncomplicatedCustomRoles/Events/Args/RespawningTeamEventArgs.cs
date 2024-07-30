using PlayerRoles;
using Respawning;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Events.Interfaces;

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
        public List<RoleTypeId> RoleQueue { get; set; }

        /// <summary>
        /// Gets or sets the max wave size
        /// </summary>
        public int MaxWaveSize { get; set; }

        /// <summary>
        /// Gets or sets the next known team
        /// </summary>
        public SpawnableTeamType NextKnownTeam { get; set; }

        public RespawningTeamEventArgs(IEnumerable<ReferenceHub> respawnQueue, IEnumerable<RoleTypeId> roleQueue, int maxSize, SpawnableTeamType nextKnownTeam)
        {
            RespawnQueue = respawnQueue.ToList();
            RoleQueue = roleQueue.ToList();
            MaxWaveSize = maxSize;
            NextKnownTeam = nextKnownTeam;
        }
    }
}
