/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Features.CustomModules;

namespace UncomplicatedCustomRoles.Events
{
    internal abstract class EventHandlerBase
    {
        internal static ConcurrentDictionary<int, Tuple<List<ItemType>, Dictionary<ItemType, ushort>, bool>> RespawnInventoryQueue { get; } = new();

        internal static HashSet<int> RagdollAppearanceQueue { get; } = new();

        internal static HashSet<int> FirstRoundPlayers { get; } = new();

        internal static ConcurrentDictionary<int, Tuple<CustomScpAnnouncer, DateTimeOffset>> TerminationQueue { get; } = new();

        internal static bool Started { get; set; } = false;

        private static readonly List<EventHandlerBase> _list = new();

        public static void Register(EventHandlerBase e)
        {
            _list.Add(e);
            e.OnRegistered();
        }

        public static void Register(IEnumerable<EventHandlerBase> e)
        {
            foreach (EventHandlerBase eventHandlerBase in e)
                Register(eventHandlerBase);
        }

        public static void Unregister(EventHandlerBase e)
        {
            e.OnUnregistered();
            _list.Remove(e);
        }

        public static void UnregisterAll()
        {
            foreach (EventHandlerBase e in _list.ToList())
                Unregister(e);
        }

        internal virtual void OnRegistered()
        { }

        internal virtual void OnUnregistered() 
        { }
    }
}