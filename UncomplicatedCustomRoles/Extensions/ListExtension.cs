/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace UncomplicatedCustomRoles.Extensions
{
    public static class ListExtension
    {
        public static void TryAdd<T>(this List<T> list, T item)
        {
            if (list == null) 
                throw new ArgumentNullException("list");

            if (!list.Contains(item))
                list.Add(item);
        }

        public static string ToRealString<T>(this List<T> list)
        {
            if (list is null)
                return "null value";

            string data = $"[{list.GetType().FullName}] List<{list.GetType().GetGenericArguments()[0].FullName}> ({list.Count}) [\n";

            foreach (T element in list)
                data += $"{element},\n";

            data += "];";

            return data;
        }

        public static T RandomValue<T>(this IEnumerable<T> list) => list.Count() < 1 ? default : list.ElementAt(UnityEngine.Random.Range(0, list.Count()));
    }
}
