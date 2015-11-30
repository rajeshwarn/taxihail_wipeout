using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using EntityFramework.Utilities;

namespace apcurium.MK.Booking.Projections
{
    public abstract class RatingTypeDetailProjectionSet : IProjectionSet<RatingTypeDetailCollection>
    {
        public abstract void Add(RatingTypeDetailCollection projection);

        public abstract void AddOrReplace(RatingTypeDetailCollection projection);

        public abstract void AddRange(IEnumerable<RatingTypeDetailCollection> projections);

        public abstract bool Exists(Guid identifier);

        public abstract bool Exists(Func<RatingTypeDetailCollection, bool> predicate);

        public abstract void Remove(Guid identifier);
        public abstract void Remove(Func<RatingTypeDetailCollection, bool> predicate);

        public abstract IProjection<RatingTypeDetailCollection> GetProjection(Guid identifier);

        public abstract void Update(Func<RatingTypeDetailCollection, bool> predicate, Action<RatingTypeDetailCollection> action);

        public abstract void Update(Guid identifier, Action<RatingTypeDetailCollection> action);
    }

    public class RatingTypeDetailMemoryProjectionSet : RatingTypeDetailProjectionSet, IEnumerable<RatingTypeDetailCollection>
    {
        readonly IDictionary<Guid, RatingTypeDetailCollection> _cache = new Dictionary<Guid, RatingTypeDetailCollection>();

        public override void Update(Func<RatingTypeDetailCollection, bool> predicate, Action<RatingTypeDetailCollection> action)
        {
            throw new NotImplementedException();
        }

        public override void Update(Guid identifier, Action<RatingTypeDetailCollection> action)
        {
            RatingTypeDetailCollection item;
            if (!_cache.TryGetValue(identifier, out item))
            {
                item = new RatingTypeDetailCollection(identifier, new List<RatingTypeDetail>());
                _cache[identifier] = item;
            }
            action.Invoke(item);
        }

        public override void Add(RatingTypeDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddOrReplace(RatingTypeDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddRange(IEnumerable<RatingTypeDetailCollection> projections)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(Func<RatingTypeDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Func<RatingTypeDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override IProjection<RatingTypeDetailCollection> GetProjection(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<RatingTypeDetailCollection> GetEnumerator()
        {
            return _cache.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class RatingTypeDetailEntityProjectionSet : RatingTypeDetailProjectionSet
    {
        readonly Func<BookingDbContext> _contextFactory;

        public RatingTypeDetailEntityProjectionSet(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override void Update(Func<RatingTypeDetailCollection, bool> predicate, Action<RatingTypeDetailCollection> action)
        {
            throw new NotImplementedException();
        }

        public override void Update(Guid identifier, Action<RatingTypeDetailCollection> action)
        {
            using (var context = _contextFactory.Invoke())
            {
                var list = Load(identifier, returnNullIfEmpty: false);
                action.Invoke(list);

                var ratingTypesLangues = list.Select(x => x.Language).ToArray();
                var ratingTypesToRemove = context.Set<RatingTypeDetail>().Where(x => x.Id == identifier && !ratingTypesLangues.Contains(x.Language));
                context.Set<RatingTypeDetail>().RemoveRange(ratingTypesToRemove);
                context.Set<RatingTypeDetail>().AddOrUpdate(list.ToArray());

                context.SaveChanges();
            }
        }

        private RatingTypeDetailCollection Load(Guid identifier, bool returnNullIfEmpty = true)
        {
            using (var context = _contextFactory.Invoke())
            {
                var ibsLinks = context.Set<RatingTypeDetail>().Where(x => x.Id == identifier).ToArray();
                if (ibsLinks.Length == 0 && returnNullIfEmpty)
                {
                    return null;
                }
                return new RatingTypeDetailCollection(identifier, ibsLinks);
            }
        }

        public override void Add(RatingTypeDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddOrReplace(RatingTypeDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddRange(IEnumerable<RatingTypeDetailCollection> projections)
        {
            using (var context = _contextFactory.Invoke())
            {
                EFBatchOperation.For(context, context.Set<RatingTypeDetail>()).InsertAll(projections.SelectMany(x => x));
            }
        }

        public override bool Exists(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(Func<RatingTypeDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Guid identifier)
        {
            using (var context = _contextFactory.Invoke())
            {
                var ratingTypes = context.Query<RatingTypeDetail>().Where(x => x.Id == identifier);
                foreach (var ratingType in ratingTypes)
                {
                    context.Set<RatingTypeDetail>().Remove(ratingType);
                }
                context.SaveChanges();
            }
        }

        public override void Remove(Func<RatingTypeDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override IProjection<RatingTypeDetailCollection> GetProjection(Guid identifier)
        {
            throw new NotImplementedException();
        }
    }

    public class RatingTypeDetailCollection : ICollection<RatingTypeDetail>
    {
        readonly List<RatingTypeDetail> _innerList;
        readonly Guid _id;

        public RatingTypeDetailCollection(Guid id, IEnumerable<RatingTypeDetail> collection)
        {
            _innerList = new List<RatingTypeDetail>(collection);
            _id = id;
        }

        public RatingTypeDetailCollection(Guid id)
            : this(id, Enumerable.Empty<RatingTypeDetail>())
        {
        }

        public Guid Id
        {
            get { return _id; }
        }

        public int Count
        {
            get
            {
                return _innerList.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(RatingTypeDetail item)
        {
            Validate(item);
            _innerList.Add(item);
        }

        private void Validate(RatingTypeDetail item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }
            if (item.Id != Id)
            {
                throw new InvalidOperationException();
            }
            if (_innerList.Any(x => x.Language == item.Language))
            {
                throw new InvalidOperationException("An item with the same id already exists");
            }
        }

        public void Clear()
        {
            _innerList.Clear();
        }

        public bool Contains(RatingTypeDetail item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(RatingTypeDetail[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public bool Remove(RatingTypeDetail item)
        {
            return _innerList.Remove(item);
        }

        public int RemoveAll(Predicate<RatingTypeDetail> match)
        {
            return _innerList.RemoveAll(match);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public IEnumerator<RatingTypeDetail> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }
    }
}