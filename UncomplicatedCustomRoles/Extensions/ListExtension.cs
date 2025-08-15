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

        public static T RandomValue<T>(this IEnumerable<T> source)
        {
            if (source is null)
                return default;

            if (source is IList<T> list)
            {
                int count = list.Count;
                if (count == 0)
                    return default;
                return list[UnityEngine.Random.Range(0, count)];
            }

            if (source is T[] arr)
            {
                if (arr.Length == 0)
                    return default;
                return arr[UnityEngine.Random.Range(0, arr.Length)];
            }

            var tmp = source.ToArray();
            if (tmp.Length == 0)
                return default;
            return tmp[UnityEngine.Random.Range(0, tmp.Length)];
        }
    }
}
