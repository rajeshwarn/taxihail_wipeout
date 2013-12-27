using System;
using System.Collections.Generic;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Framework.Extensions
{
    /// <summary>
    ///     Provides Extensions Methods for ICollection.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        ///     Adds a new item with the default constructor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static T AddNew<T>(this ICollection<T> items)
            where T : new()
        {
            var item = new T();

            items.Add(item);

            return item;
        }

        /// <summary>
        ///     Adds the items of the specified collection to the end of the ICollection.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="collection">Collection in which to insert items.</param>
        /// <param name="items">The items to add.</param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            items.ForEach(collection.Add);
        }

        /// <summary>
        ///     Removes items in a collection that are identified with a predicate.
        /// </summary>
        /// <typeparam name="T">the type of the items</typeparam>
        /// <param name="collection">Collection in which to remove items.</param>
        /// <param name="predicate">The predicate used to identify if a item is to be removed or not.</param>
        public static void Remove<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            collection
                .Where(predicate)
                .ToList()
                .ForEach(item => collection.Remove(item));
        }


        /// <summary>
        ///     Replaces the items in a collection with a new set of items.
        /// </summary>
        /// <typeparam name="T">The type of items.</typeparam>
        /// <param name="collection">The collection who's content will be replaced.</param>
        /// <param name="items">The replacing items.</param>
        public static void ReplaceWith<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();
            AddRange(collection, items);
        }

        public static void AddDistinct<T>(this ICollection<T> collection, T item, IEqualityComparer<T> comparer)
        {
            AddDistinct(collection, item, comparer.Equals);
        }

        public static void AddDistinct<T>(this ICollection<T> collection, T item, Func<T, T, bool> predicate)
        {
            if (collection.None(collectionItem => predicate(collectionItem, item)))
            {
                collection.Add(item);
            }
        }


        public static void AddRangeDistinct<T>(this ICollection<T> collection, IEnumerable<T> items,
            IEqualityComparer<T> comparer)
        {
            AddRangeDistinct(collection, items, comparer.Equals);
        }

        public static void AddRangeDistinct<T>(this ICollection<T> collection, IEnumerable<T> items,
            Func<T, T, bool> predicate)
        {
            items.ForEach(item => collection.AddDistinct(item, predicate));
        }

        public static T FindOrCreate<T>(this ICollection<T> collection, Func<T, bool> predicate, Func<T> factory)
            where T : class
        {
            T value = collection.FirstOrDefault(predicate);

            if (value == null)
            {
                value = factory();
                collection.Add(value);
            }

            return value;
        }
    }
}