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
    public abstract class AccountIbsDetailProjectionSet : IProjectionSet<AccountIbsDetailCollection>
    {
        public abstract void Add(AccountIbsDetailCollection projection);

        public abstract void AddOrReplace(AccountIbsDetailCollection projection);

        public abstract void AddRange(IEnumerable<AccountIbsDetailCollection> projections);

        public abstract bool Exists(Guid identifier);

        public abstract bool Exists(Func<AccountIbsDetailCollection, bool> predicate);

        public abstract void Remove(Guid identifier);
        public abstract void Remove(Func<AccountIbsDetailCollection, bool> predicate);

        public abstract IProjection<AccountIbsDetailCollection> GetProjection(Guid identifier);

        public abstract void Update(Func<AccountIbsDetailCollection, bool> predicate, Action<AccountIbsDetailCollection> action);

        public abstract void Update(Guid identifier, Action<AccountIbsDetailCollection> action);
    }

    public class AccountIbsDetailMemoryProjectionSet : AccountIbsDetailProjectionSet, IEnumerable<AccountIbsDetailCollection>
    {
        readonly IDictionary<Guid, AccountIbsDetailCollection> _cache = new Dictionary<Guid, AccountIbsDetailCollection>();

        public override void Update(Func<AccountIbsDetailCollection, bool> predicate, Action<AccountIbsDetailCollection> action)
        {
            throw new NotImplementedException();
        }

        public override void Update(Guid identifier, Action<AccountIbsDetailCollection> action)
        {
            AccountIbsDetailCollection item;
            if (!_cache.TryGetValue(identifier, out item))
            {
                item = new AccountIbsDetailCollection(identifier, new List<AccountIbsDetail>());
                _cache[identifier] = item;
            }
            action.Invoke(item);
        }

        public override void Add(AccountIbsDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddOrReplace(AccountIbsDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddRange(IEnumerable<AccountIbsDetailCollection> projections)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(Func<AccountIbsDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Func<AccountIbsDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override IProjection<AccountIbsDetailCollection> GetProjection(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<AccountIbsDetailCollection> GetEnumerator()
        {
            return _cache.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class AccountIbsDetailEntityProjectionSet : AccountIbsDetailProjectionSet
    {
        readonly Func<BookingDbContext> _contextFactory;

        public AccountIbsDetailEntityProjectionSet(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override void Update(Func<AccountIbsDetailCollection, bool> predicate, Action<AccountIbsDetailCollection> action)
        {
            throw new NotImplementedException();
        }

        public override void Update(Guid identifier, Action<AccountIbsDetailCollection> action)
        {
            using (var context = _contextFactory.Invoke())
            {
                var list = Load(identifier, returnNullIfEmpty: false);
                action.Invoke(list);

                var ibsLinksCompanyKeys = list.Select(x => x.CompanyKey).ToArray();
                var ibsLinksToRemove = context.Set<AccountIbsDetail>().Where(x => x.AccountId == identifier && !ibsLinksCompanyKeys.Contains(x.CompanyKey));
                context.Set<AccountIbsDetail>().RemoveRange(ibsLinksToRemove);
                context.Set<AccountIbsDetail>().AddOrUpdate(list.ToArray());

                context.SaveChanges();
            }
        }

        private AccountIbsDetailCollection Load(Guid identifier, bool returnNullIfEmpty = true)
        {
            using (var context = _contextFactory.Invoke())
            {
                var ibsLinks = context.Set<AccountIbsDetail>().Where(x => x.AccountId == identifier).ToArray();
                if (ibsLinks.Length == 0 && returnNullIfEmpty)
                {
                    return null;
                }
                return new AccountIbsDetailCollection(identifier, ibsLinks);
            }
        }

        public override void Add(AccountIbsDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddOrReplace(AccountIbsDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddRange(IEnumerable<AccountIbsDetailCollection> projections)
        {
            using (var context = _contextFactory.Invoke())
            {
                EFBatchOperation.For(context, context.Set<AccountIbsDetail>()).InsertAll(projections.SelectMany(x => x));
            }
        }

        public override bool Exists(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(Func<AccountIbsDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Func<AccountIbsDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override IProjection<AccountIbsDetailCollection> GetProjection(Guid identifier)
        {
            throw new NotImplementedException();
        }
    }

    public class AccountIbsDetailCollection : ICollection<AccountIbsDetail>
    {
        readonly List<AccountIbsDetail> _innerList;
        readonly Guid _accountId;

        public AccountIbsDetailCollection(Guid accountId, IEnumerable<AccountIbsDetail> collection)
        {
            _innerList = new List<AccountIbsDetail>(collection);
            _accountId = accountId;
        }

        public AccountIbsDetailCollection(Guid accountId)
            : this(accountId, Enumerable.Empty<AccountIbsDetail>())
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

        public void Add(AccountIbsDetail item)
        {
            Validate(item);
            _innerList.Add(item);
        }

        private void Validate(AccountIbsDetail item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }
            if (item.AccountId != AccountId)
            {
                throw new InvalidOperationException();
            }
            if (_innerList.Any(x => x.CompanyKey == item.CompanyKey))
            {
                throw new InvalidOperationException("An item with the same id already exists");
            }
        }

        public void Clear()
        {
            _innerList.Clear();
        }

        public bool Contains(AccountIbsDetail item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(AccountIbsDetail[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public bool Remove(AccountIbsDetail item)
        {
            return _innerList.Remove(item);
        }

        public int RemoveAll(Predicate<AccountIbsDetail> match)
        {
            return _innerList.RemoveAll(match);

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }
        public IEnumerator<AccountIbsDetail> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }
    }
}