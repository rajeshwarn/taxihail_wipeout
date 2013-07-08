﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace apcurium.MK.Common.Extensions
{
    /// <summary>
    /// Provides Extensions Methods for IEnumerable.
    /// </summary>
    public static class EnumerableExtensions
    {        

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
		
		public static bool HasValue<T> (this IEnumerable<T> items)
        {			
            return  ( (items != null) && (items.Count() > 0 ) );
        }
        public static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            return !items.Any(predicate);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> items, Func<T, bool> predicate=null)
        {
            if (predicate == null)
            {
                return !items.Any();
            }
            return !items.Any(predicate);
        }

        static Random random = new Random();
        public static T GetRandom<T>(this T[] items)
        {
            var index = random.Next(0, items.Length);
            return items[index];
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

            foreach (var instance in items)
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

       	public static bool Length<T> (this IEnumerable<T> items, int length)
        {			
            return  ( (items.HasValue() ) && ( items.Count() == length ) );
        }
       
    }
}