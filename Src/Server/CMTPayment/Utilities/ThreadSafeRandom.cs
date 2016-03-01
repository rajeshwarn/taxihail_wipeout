using System;

namespace CMTPayment.Utilities
{
    internal class ThreadSafeRandom
    {
        private readonly Random _randomProvider;

        private readonly object _lock = new object();

        public ThreadSafeRandom(int? seed = null)
        {
            _randomProvider = seed.HasValue
                ? new Random(seed.Value)
                : new Random();
        }

        public int Next()
        {
            lock (_lock)
            {
                return _randomProvider.Next();
            }
        }

        public int Next(int maxValue)
        {
            lock (_lock)
            {
                return _randomProvider.Next(maxValue);
            }
        }

        public int Next(int minValue, int maxValue)
        {
            lock (_lock)
            {
                return _randomProvider.Next(minValue, maxValue);
            }
        }

        public double NextDouble()
        {
            lock (_lock)
            {
                return _randomProvider.NextDouble();
            }
        }
    }
}
