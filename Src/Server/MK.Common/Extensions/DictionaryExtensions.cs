using System;
using System.Collections.Generic;
using System.Linq;

namespace apcurium.MK.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue FindOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> items, TKey key,
                                                        Func<TValue> factory)
        {
            TValue value;

            if (!items.TryGetValue(key, out value))
            {
                value = factory();
                items.Add(key, value);
            }

            return value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return GetValueOrDefault(dictionary, key, default(TValue));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            var value = defaultValue;

            if (dictionary != null)
            {
                if (!dictionary.TryGetValue(key, out value))
                    value = defaultValue;
            }

            return value;
        }

        public static IEnumerable<TKey> RemoveKeys<TKey, TValue>(this IDictionary<TKey, TValue> items, IEnumerable<TKey> range)
        {
            return range.Where(k => items.Remove(k)).ToList();
        }

        public static T MergeLeft<T, K, V>(this T me, params IDictionary<K, V>[] others)
        where T : IDictionary<K, V>, new()
        {
            T newMap = new T();
            foreach (IDictionary<K, V> src in
                (new List<IDictionary<K, V>> { me }).Concat(others))
            {
                // ^-- echk. Not quite there type-system.
                foreach (KeyValuePair<K, V> p in src)
                {
                    newMap[p.Key] = p.Value;
                }
            }
            return newMap;
        }
    }
}