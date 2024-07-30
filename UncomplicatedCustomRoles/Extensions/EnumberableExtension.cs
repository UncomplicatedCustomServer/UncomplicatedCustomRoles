using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UncomplicatedCustomRoles.Extensions
{
    public static class EnumberableExtension
    {
        public static void TryAdd<T>(this IEnumerable<T> list, T item)
        {
            if (list == null) 
                throw new ArgumentNullException("list");

            if (!list.Contains(item))
                list.AddItem(item);
        }

        public static T RandomValue<T>(this IEnumerable<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            return list.ElementAtOrDefault(new Random().Next(0, list.Count() - 1));
        }
    }
}
