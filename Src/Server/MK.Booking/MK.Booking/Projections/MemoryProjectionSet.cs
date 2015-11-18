﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Projections
{
    public class MemoryProjectionSet<TProjection>:
        IProjectionSet<TProjection>,
        IEnumerable<TProjection> where TProjection : class
    {
        readonly IDictionary<Guid, TProjection> _cache = new Dictionary<Guid, TProjection>();
        readonly Func<TProjection, Guid> _getId;
        public MemoryProjectionSet(Func<TProjection, Guid> getId)
        {
            _getId = getId;
        }

        public void Add(TProjection projection)
        {
            _cache.Add(_getId(projection), projection);
        }

        public void AddOrReplace(TProjection projection)
        {
            _cache[_getId(projection)] = projection;
        }

        public void AddRange(IEnumerable<TProjection> projections)
        {
            foreach (var projection in projections)
            {
                Add(projection);
            }
        }

        public bool Exists(Guid sourceId)
        {
            return _cache.ContainsKey(sourceId);
        }

        public IProjection<TProjection> GetProjection(Guid identifier)
        {
            return new ProjectionWrapper(() =>
            {
                TProjection p;
                return _cache.TryGetValue(identifier, out p) ? p : default(TProjection);
            }, projection => _cache[identifier] = projection);
        }

        public void Update(Guid identifier, Action<TProjection> action)
        {
            TProjection item;
            if (!_cache.TryGetValue(identifier, out item))
            {
                throw new InvalidOperationException("Projection not found");
            }
            action.Invoke(item);
        }

        public void Update(Func<TProjection, bool> predicate, Action<TProjection> action)
        {
            foreach (var item in _cache
                .Select(x => x.Value)
                .Where(predicate))
            {
                action.Invoke(item);
            }
        }

        IEnumerator<TProjection> IEnumerable<TProjection>.GetEnumerator()
        {
            return _cache.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cache.Values.GetEnumerator();
        }

        private class ProjectionWrapper : IProjection<TProjection>
        {
            private Func<TProjection> _load;
            private Action<TProjection> _save;
            public ProjectionWrapper(Func<TProjection> load, Action<TProjection> save)
            {
                _load = load;
                _save = save;
            }

            public TProjection Load()
            {
                return _load();
            }

            public void Save(TProjection projection)
            {
                _save(projection);
            }
        }


    }
}
