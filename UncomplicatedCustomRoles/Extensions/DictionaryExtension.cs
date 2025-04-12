﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

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
