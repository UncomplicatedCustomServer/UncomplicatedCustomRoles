using System;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.Extensions
{
    public static class ListExtension
    {
        public static void TryAdd<T>(this List<T> list, T item)
        {
            if (list is null) 
                throw new ArgumentNullException("list");

            if (!list.Contains(item))
                list.Add(item);
        }

        public static void TryPush<T>(this List<T> list, int index, T item)
        {
            if (list is null)
                throw new ArgumentNullException("list");

            if (index >= list.Count)
                list.Add(item);
            else
                list[index] = item;
        }
    }
}
