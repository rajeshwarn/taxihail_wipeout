using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Framework.Extensions
{
    /// <summary>
    ///     Provides Extensions Methods for IEnumerable.
    /// </summary>
    public static class EnumerableExtensions
    {
        //public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<KeyValuePair<int, T>> action)
        //{
        //    return ForEach(items, action.ToAction());
        //}

        //public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<int, T> action)
        //{
        //    if (items != null)
        //    {
        //        var i = 0;

        //        foreach (var item in items)
        //        {
        //            action(i++, item);
        //        }
        //    }

        //    return items;
        //}

        //public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items)
        //{
        //    if (items != null)
        //    {
        //        foreach (var item in items)
        //        {
        //        }
        //    }

        //    return items;
        //}

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items != null)
            {
                foreach (T item in items)
                {
                    action(item);
                }
            }

            return items;
        }

        public static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            return !items.Any(predicate);
        }

        public static bool Empty<T>(this IEnumerable<T> items)
        {
            return !items.Any();
        }

        public static object[] ToObjectArray(this IEnumerable items)
        {
            return items.Cast<object>().ToArray();
        }


        public static int IndexOf<T>(this IEnumerable<T> items, T item, IEqualityComparer<T> comparer)
        {
            return IndexOf(items, item, comparer.Equals);
        }

        public static int IndexOf<T>(this IEnumerable<T> items, T item, Func<T, T, bool> predicate)
        {
            var index = 0;

            foreach (T instance in items)
            {
                if (predicate(item, instance))
                {
                    return index;
                }

                ++index;
            }

            return -1;
        }

        public static bool AreDistinct<T>(this IEnumerable<T> items)
        {
            return items.Count() == items.Distinct().Count();
        }


        public static TResult SingleOrDefault<T, TResult>(this IEnumerable<T> items, Func<T, TResult> selector)
        {
            T result = items.SingleOrDefault();

            return result.Extensions().IsDefault() ? default(TResult) : selector(result);
        }
    }
}