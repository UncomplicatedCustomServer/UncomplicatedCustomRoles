using Exiled.API.Features;
using MEC;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.API.Features
{
    internal class Escape
    {
        /// <summary>
        /// Gets the escape bucket to avoid the spam of SubclassSpawn of a custom role during the spawn
        /// </summary>
        public static List<int> Bucket { get; } = new();

        public static void AddBucket(Player player, float waitingTime = 5f)
        {
            Bucket.TryAdd(player.Id);
            Timing.CallDelayed(waitingTime, () => Bucket.Remove(player.Id));
        }
    }
}
