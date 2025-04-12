/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using HarmonyLib;
using System;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.Extensions
{
    public static class DictionaryExtension
    {
        public static void TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey Key, TValue value)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.ContainsKey(Key))
                dictionary[Key] = value;
            else
                dictionary.Add(Key, value);
        }

        public static TValue TryGetElement<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue ifNot)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.ContainsKey(key))
                return dictionary[key];

            return ifNot;
        }

        public static void TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary is null)
                throw new ArgumentNullException(nameof(dictionary));

            if (dictionary.ContainsKey(key))
                dictionary.Remove(key);
        }

        public static string ToRealString<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary is null)
                return string.Empty;

            string Data = $"[{dictionary.GetType().FullName}] Dictionary<{dictionary.GetType().GetGenericArguments()[0].FullName}, {dictionary.GetType().GetGenericArguments()[1].FullName}> ({dictionary.Count}) [\n";

            foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
                Data += $"{kvp.Key}: {kvp.Value},\n";

            Data += "];";

            return Data;
        }

        public static Dictionary<string, string> ConvertToString<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            Dictionary<string, string> result = new();

            foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
                result.Add(kvp.Key.ToString(), kvp.Value.ToString());

            return result;
        }

        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            Dictionary<TKey, TValue> newDictionary = new();

            foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
                newDictionary.AddItem(kvp);

            return newDictionary;
        }
    }
}
