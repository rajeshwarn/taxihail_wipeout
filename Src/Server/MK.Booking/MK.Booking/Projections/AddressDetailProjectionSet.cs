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
    public abstract class AddressDetailProjectionSet : IProjectionSet<AddressDetailCollection>
    {
        public abstract void Add(AddressDetailCollection projection);

        public abstract void AddOrReplace(AddressDetailCollection projection);

        public abstract void AddRange(IEnumerable<AddressDetailCollection> projections);

        public abstract bool Exists(Guid identifier);

        public abstract bool Exists(Func<AddressDetailCollection, bool> predicate);

        public abstract void Remove(Guid identifier);
        public abstract void Remove(Func<AddressDetailCollection, bool> predicate);

        public abstract IProjection<AddressDetailCollection> GetProjection(Guid identifier);

        public abstract void Update(Func<AddressDetailCollection, bool> predicate, Action<AddressDetailCollection> action);

        public abstract void Update(Guid identifier, Action<AddressDetailCollection> action);
    }

    public class AddressDetailMemoryProjectionSet : AddressDetailProjectionSet, IEnumerable<AddressDetailCollection>
    {
        readonly IDictionary<Guid, AddressDetailCollection> _cache = new Dictionary<Guid, AddressDetailCollection>();

        public override void Add(AddressDetailCollection projection)
        {
            if (projection == null)
            {
                throw new ArgumentNullException();
            }

            _cache.Add(projection.Identifier, projection);
        }

        public override void AddOrReplace(AddressDetailCollection projection)
        {
            if (projection == null)
            {
                throw new ArgumentNullException();
            }

            _cache[projection.Identifier] = projection;
        }

        public override void AddRange(IEnumerable<AddressDetailCollection> projections)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(Guid identifier)
        {
            return _cache.ContainsKey(identifier);
        }

        public override bool Exists(Func<AddressDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Func<AddressDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override IProjection<AddressDetailCollection> GetProjection(Guid identifier)
        {
            return new ProjectionWrapper<AddressDetailCollection>(() =>
            {
                AddressDetailCollection p;
                return _cache.TryGetValue(identifier, out p) ? p : default(AddressDetailCollection);
            },
            projection =>
            {
                _cache[identifier] = projection;
            });
        }

        public override void Update(Guid identifier, Action<AddressDetailCollection> action)
        {
            AddressDetailCollection item;
            if (!_cache.TryGetValue(identifier, out item))
            {
                throw new InvalidOperationException("Projection not found");
            }
            action.Invoke(item);
        }

        public override void Update(Func<AddressDetailCollection, bool> predicate, Action<AddressDetailCollection> action)
        {
            foreach (var item in _cache
               .Select(x => x.Value)
               .Where(predicate))
            {
                action.Invoke(item);
            }
        }

        public IEnumerator<AddressDetailCollection> GetEnumerator()
        {
            return _cache.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cache.Values.GetEnumerator();
        }
    }

    public class AddressDetailEntityProjectionSet : AddressDetailProjectionSet
    {
        readonly Func<BookingDbContext> _contextFactory;
        public AddressDetailEntityProjectionSet(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public override void Add(AddressDetailCollection projection)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Set<AddressDetails>().AddRange(projection);
                context.SaveChanges();
            }
        }

        public override void AddOrReplace(AddressDetailCollection projection)
        {
            throw new NotImplementedException();
        }

        public override void AddRange(IEnumerable<AddressDetailCollection> projections)
        {
            using (var context = _contextFactory.Invoke())
            {
                EFBatchOperation.For(context, context.Set<AddressDetails>()).InsertAll(projections.SelectMany(x => x));
            }
        }

        public override bool Exists(Guid identifier)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Set<AddressDetails>().Any(x => x.AccountId == identifier);
            }
        }

        public override bool Exists(Func<AddressDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Guid identifier)
        {
            throw new NotImplementedException();
        }

        public override void Remove(Func<AddressDetailCollection, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public override IProjection<AddressDetailCollection> GetProjection(Guid identifier)
        {
            return new ProjectionWrapper<AddressDetailCollection>(() => Load(identifier), p =>
            {
                using (var context = _contextFactory.Invoke())
                {
                    var addressIds = p.Select(x => x.Id).ToArray();
                    var addressesToRemove = context.Set<AddressDetails>().Where(x => x.AccountId == identifier && !addressIds.Contains(x.Id));
                    context.Set<AddressDetails>().RemoveRange(addressesToRemove);
                    context.Set<AddressDetails>().AddOrUpdate(p.ToArray());
                }
            });
        }

        public override void Update(Guid identifier, Action<AddressDetailCollection> action)
        {
            using (var context = _contextFactory.Invoke())
            {
                var list = Load(identifier, returnNullIfEmpty: false);
                action.Invoke(list);

                var addressIds = list.Select(x => x.Id).ToArray();
                var addressesToRemove = context.Set<AddressDetails>().Where(x => x.AccountId == identifier && !addressIds.Contains(x.Id));
                context.Set<AddressDetails>().RemoveRange(addressesToRemove);
                context.Set<AddressDetails>().AddOrUpdate(list.ToArray());

                context.SaveChanges();
            }
        }

        public override void Update(Func<AddressDetailCollection, bool> predicate, Action<AddressDetailCollection> action)
        {
            throw new NotImplementedException();
        }

        private AddressDetailCollection Load(Guid identifier, bool returnNullIfEmpty = true)
        {
            using (var context = _contextFactory.Invoke())
            {
                var addresses = context.Set<AddressDetails>().Where(x => x.AccountId == identifier).ToArray();
                if (addresses.Length == 0 && returnNullIfEmpty)
                {
                    return null;
                }
                return new AddressDetailCollection(identifier, addresses);
            }
        }
    }

    public class AddressDetailCollection : ICollection<AddressDetails>
    {
        readonly List<AddressDetails> _innerList;
        readonly Guid _identifier;

        public AddressDetailCollection(Guid identifier, IEnumerable<AddressDetails> collection)
        {
            _innerList = new List<AddressDetails>(collection);
            _identifier = identifier;
        }

        public AddressDetailCollection(Guid identifier)
            : this(identifier, Enumerable.Empty<AddressDetails>())
        {
        }

        public Guid Identifier
        {
            get { return _identifier; }
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

        public void Add(AddressDetails item)
        {
            Validate(item);
            _innerList.Add(item);
        }

        private void Validate(AddressDetails item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }
            if (item.AccountId != Identifier)
            {
                throw new InvalidOperationException();
            }
            if (_innerList.Any(x => x.Id == item.Id))
            {
                throw new InvalidOperationException("An item with the same id already exists");
            }
        }

        public void Clear()
        {
            _innerList.Clear();
        }

        public bool Contains(AddressDetails item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(AddressDetails[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public bool Remove(AddressDetails item)
        {
            return _innerList.Remove(item);
        }

        public int RemoveAll(Predicate<AddressDetails> match)
        {
            return _innerList.RemoveAll(match);

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }
        public IEnumerator<AddressDetails> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }
    }
}