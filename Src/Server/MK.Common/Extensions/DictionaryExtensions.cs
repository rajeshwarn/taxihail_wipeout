#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

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

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            TValue defaultValue)
        {
            var value = defaultValue;

            if (dictionary != null)
            {
                if (!dictionary.TryGetValue(key, out value))
                    value = defaultValue;
            }

            return value;
        }

        public static IEnumerable<TKey> RemoveKeys<TKey, TValue>(this IDictionary<TKey, TValue> items,
            IEnumerable<TKey> range)
        {
            return range.Where(items.Remove).ToList();
        }

        public static T MergeLeft<T, TK, TV>(this T me, params IDictionary<TK, TV>[] others)
            where T : IDictionary<TK, TV>, new()
        {
            var newMap = new T();
            foreach (var src in
                (new List<IDictionary<TK, TV>> {me}).Concat(others))
            {
                // ^-- echk. Not quite there type-system.
                foreach (var p in src)
                {
                    newMap[p.Key] = p.Value;
                }
            }
            return newMap;
        }
    }
}