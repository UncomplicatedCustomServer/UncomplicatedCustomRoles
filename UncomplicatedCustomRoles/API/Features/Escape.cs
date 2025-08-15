/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using LabApi.Features.Wrappers;
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
        public static HashSet<int> Bucket { get; } = new();

        public static void AddBucket(Player player, float waitingTime = 5f)
        {
            Bucket.Add(player.PlayerId);
            Timing.CallDelayed(waitingTime, () => Bucket.Remove(player.PlayerId));
        }
    }
}
