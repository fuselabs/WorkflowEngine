// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Linq;

namespace WorkflowEngine.Utils
{
    public static class DictUtils
    {
        public static void SafeAdd<TKey, TValue>(this IDictionary<TKey, ICollection<TValue>> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key].Add(value);
            }
            else
            {
                dictionary.Add(key, new HashSet<TValue> { value });
            }
        }

        /// <summary>
        /// Returns the key in the dictionary whose value is maximum
        /// If there is a tie, all ties are returned
        /// </summary>
        public static IEnumerable<TKey> ArgMax<TKey, TValue>(this IDictionary<TKey, IList<TValue>> dictionary)
        {
            var maxKeyCount = dictionary.Max(pair => pair.Value.Count);
            return dictionary.Where(pair => pair.Value.Count == maxKeyCount).Select(pair => pair.Key);
        }
    }
}
