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
    public abstract class PromotionProgressDetailProjectionSet : IProjectionSet<PromotionProgressDetailCollection>
    {
        public abstract void Add(PromotionProgressDetailCollection projection);

        public abstract void AddOrReplace(PromotionProgressDetailCollection projection);

        public abstract void AddRange(IEnumerable<PromotionProgressDetailCollection> projections);

        public abstract bool Exists(Guid identifier);

        public abstract bool Exists(Func<PromotionProgressDetailCollection, bool> predicate);

        public abstract void Remove(Guid identifier);
        public abstract void Remove(Func<PromotionProgressDetailCollection, bool> predicate);

        public abstract IProjection<PromotionProgressDetailCollection> GetProjection(Guid identifier);
        public abstract IProjection<PromotionProgressDetailCollection> GetProjection(Func<PromotionProgressDetailCollection, bool> predicate);

        public abstract void Update(Func<PromotionProgressDetailCollection, bool> predicate, Action<PromotionProgressDetailCollection> action);

        public abstract void Update(Guid identifier, Action<PromotionProgressDetailCollection> action);
    }

    public class PromotionProgressDetailMemoryProjectionSet : PromotionProgressDetailProjectionSet, IEnumerable<PromotionProgressDetailCollection>
    {
        readonly IDictionary<Guid, PromotionProgressDetailCollection> _cache = new Dictionary<Guid, PromotionProgressDetailCollection>();

        public override IProjection<PromotionProgressDetailCollection> GetProjection(Func<PromotionProgressDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override void Update(Func<PromotionProgressDetailCollection, bool> predicate, Action<PromotionProgressDetailCollection> action)
        {
            throw new NotImplementedException();
        }

        public override void Update(Guid identifier, Action<PromotionProgressDetailCollection> action)
        {
            PromotionProgressDetailCollection item;
            if (!_cache.TryGetValue(identifier, out item))
            {
                item = new PromotionProgressDetailCollection(identifier, new List<PromotionProgressDetail>());
                _cache[identifier] = item;
            }
            action.Invoke(item);
        }

        public override void Add(PromotionProgressDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddOrReplace(PromotionProgressDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddRange(IEnumerable<PromotionProgressDetailCollection> projections)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(Func<PromotionProgressDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Func<PromotionProgressDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override IProjection<PromotionProgressDetailCollection> GetProjection(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<PromotionProgressDetailCollection> GetEnumerator()
        {
            return _cache.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class PromotionProgressDetailEntityProjectionSet : PromotionProgressDetailProjectionSet
    {
        readonly Func<BookingDbContext> _contextFactory;

        public PromotionProgressDetailEntityProjectionSet(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override IProjection<PromotionProgressDetailCollection> GetProjection(Func<PromotionProgressDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override void Update(Func<PromotionProgressDetailCollection, bool> predicate, Action<PromotionProgressDetailCollection> action)
        {
            throw new NotImplementedException();
        }

        public override void Update(Guid identifier, Action<PromotionProgressDetailCollection> action)
        {
            using (var context = _contextFactory.Invoke())
            {
                var list = Load(identifier, returnNullIfEmpty: false);
                action.Invoke(list);

                var promoIds = list.Select(x => x.PromoId).ToArray();
                var promoProgressToRemove = context.Set<PromotionProgressDetail>().Where(x => x.AccountId == identifier && !promoIds.Contains(x.PromoId));
                context.Set<PromotionProgressDetail>().RemoveRange(promoProgressToRemove);
                context.Set<PromotionProgressDetail>().AddOrUpdate(list.ToArray());

                context.SaveChanges();
            }
        }

        private PromotionProgressDetailCollection Load(Guid identifier, bool returnNullIfEmpty = true)
        {
            using (var context = _contextFactory.Invoke())
            {
                var promoProgress = context.Set<PromotionProgressDetail>().Where(x => x.AccountId == identifier).ToArray();
                if (promoProgress.Length == 0 && returnNullIfEmpty)
                {
                    return null;
                }
                return new PromotionProgressDetailCollection(identifier, promoProgress);
            }
        }

        public override void Add(PromotionProgressDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddOrReplace(PromotionProgressDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddRange(IEnumerable<PromotionProgressDetailCollection> projections)
        {
            using (var context = _contextFactory.Invoke())
            {
                EFBatchOperation.For(context, context.Set<PromotionProgressDetail>()).InsertAll(projections.SelectMany(x => x));
            }
        }

        public override bool Exists(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(Func<PromotionProgressDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Func<PromotionProgressDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override IProjection<PromotionProgressDetailCollection> GetProjection(Guid identifier)
        {
            throw new NotImplementedException();
        }
    }

    public class PromotionProgressDetailCollection : ICollection<PromotionProgressDetail>
    {
        readonly List<PromotionProgressDetail> _innerList;
        readonly Guid _accountId;

        public PromotionProgressDetailCollection(Guid accountId, IEnumerable<PromotionProgressDetail> collection)
        {
            _innerList = new List<PromotionProgressDetail>(collection);
            _accountId = accountId;
        }

        public PromotionProgressDetailCollection(Guid accountId)
            : this(accountId, Enumerable.Empty<PromotionProgressDetail>())
        {
        }

        public Guid AccountId
        {
            get { return _accountId; }
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

        public void Add(PromotionProgressDetail item)
        {
            Validate(item);
            _innerList.Add(item);
        }

        private void Validate(PromotionProgressDetail item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }
            if (item.AccountId != AccountId)
            {
                throw new InvalidOperationException();
            }
            if (_innerList.Any(x => x.PromoId == item.PromoId))
            {
                throw new InvalidOperationException("An item with the same id already exists");
            }
        }

        public void Clear()
        {
            _innerList.Clear();
        }

        public bool Contains(PromotionProgressDetail item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(PromotionProgressDetail[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public bool Remove(PromotionProgressDetail item)
        {
            return _innerList.Remove(item);
        }

        public int RemoveAll(Predicate<PromotionProgressDetail> match)
        {
            return _innerList.RemoveAll(match);

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }
        public IEnumerator<PromotionProgressDetail> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }
    }
}