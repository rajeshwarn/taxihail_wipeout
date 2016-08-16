using System.Collections.Generic;

namespace apcurium.MK.Common.Collections
{
    /// <summary>
    /// List that provides thread-safe operators.
    /// </summary>
    /// <typeparam name="T">SynchronizedList items type.</typeparam>
    public class SynchronizedList<T> : List<T>
    {
        private readonly object _operatorLock = new object();

        /// <summary>
        /// Add the specified item to the list in a thread-safe way.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public new void Add(T item)
        {
            lock (_operatorLock)
            {
                base.Add(item);
            }
        }

        /// <summary>
        /// Remove the specified item from the list in a thread-safe way.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public new bool Remove(T item)
        {
            lock (_operatorLock)
            {
                return base.Remove(item);
            }
        }
    }
}