﻿/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features
{
    public class Spawn
    {
        /// <summary>
        /// Whether the next respawn wave should be handled by UCR
        /// </summary>
        public static bool DoHandleWave { get; internal set; } = true;

        /// <summary>
        /// Gets the list of every player Id that will be spawned in the next wave
        /// </summary>
        public static List<int> SpawnQueue { get; } = new();

        /// <summary>
        /// Gets a list of players that are being spawned - in this way we don't trigger the pugin
        /// </summary>
        internal static List<int> Spawning { get; } = new();

        /// <summary>
        /// Disable the UCR next respawn wave evaluation
        /// </summary>
        public static void DisableSpawnWave()
        {
            DoHandleWave = false;
            SpawnQueue.Clear();
        }
    }
}
