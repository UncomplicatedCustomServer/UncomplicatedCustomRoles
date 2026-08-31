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
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.Manager;

internal static class InventoryLimitOverride
{
    private static readonly ConcurrentDictionary<int, Dictionary<ItemCategory, sbyte>> Categories = new();

    internal static void Set(int playerId, ItemCategory category, sbyte limit)
    {
        Categories.GetOrAdd(playerId, _ => new Dictionary<ItemCategory, sbyte>())[category] = limit;
    }

    internal static void Clear(int playerId, ItemCategory category)
    {
        if (!Categories.TryGetValue(playerId, out Dictionary<ItemCategory, sbyte> map))
            return;

        map.Remove(category);
        if (map.Count == 0)
            Categories.TryRemove(playerId, out _);
    }

    internal static void ClearAll()
    {
        Categories.Clear();
    }

    internal static void ClearAll(int playerId)
    {
        Categories.TryRemove(playerId, out _);
    }

    internal static bool TryGet(int playerId, ItemCategory category, out sbyte limit)
    {
        limit = 0;
        return Categories.TryGetValue(playerId, out Dictionary<ItemCategory, sbyte> map) && map.TryGetValue(category, out limit);
    }
}