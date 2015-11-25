#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using AutoMapper;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Projections;
using System.Collections.Generic;
using System.Collections;
using System.Data.Entity.Migrations;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class AddressListGenerator : 
        IEventHandler<FavoriteAddressAdded>, 
        IEventHandler<FavoriteAddressRemoved>,
        IEventHandler<FavoriteAddressUpdated>, 
        IEventHandler<OrderCreated>, 
        IEventHandler<AddressRemovedFromHistory>,
        IEventHandler<DefaultFavoriteAddressAdded>, 
        IEventHandler<DefaultFavoriteAddressRemoved>,
        IEventHandler<DefaultFavoriteAddressUpdated>,
        IEventHandler<PopularAddressAdded>, 
        IEventHandler<PopularAddressRemoved>,
        IEventHandler<PopularAddressUpdated>, 
        IEventHandler<AccountRegistered>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly AddressDetailProjectionSet _addressDetailProjectionSet;

        public AddressListGenerator(Func<BookingDbContext> contextFactory, AddressDetailProjectionSet addressDetailProjectionSet)
        {
            _contextFactory = contextFactory;
            _addressDetailProjectionSet = addressDetailProjectionSet;
        }

        public void Handle(AddressRemovedFromHistory @event)
        {
            _addressDetailProjectionSet.Update(@event.SourceId, list => {
                list.RemoveAll(x => x.Id == @event.AddressId);
            });
        }

        public void Handle(DefaultFavoriteAddressAdded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = new DefaultAddressDetails();
                Mapper.Map(@event.Address, address);
                context.Save(address);
            }
        }

        public void Handle(DefaultFavoriteAddressRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<DefaultAddressDetails>(@event.AddressId);
                if (address != null)
                {
                    context.Set<DefaultAddressDetails>().Remove(address);
                    context.SaveChanges();
                }
            }
        }

        public void Handle(DefaultFavoriteAddressUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<DefaultAddressDetails>(@event.Address.Id);
                if (address != null)
                {
                    Mapper.Map(@event.Address, address);
                    context.SaveChanges();
                }
            }
        }

        public void Handle(FavoriteAddressAdded @event)
        {
            _addressDetailProjectionSet.Update(@event.SourceId, list => {

                var existingAddress = list.SingleOrDefault(x => x.Id == @event.Address.Id);
                if (existingAddress != null)
                {
                    // TODO: Log this problem
                    // Address already exist, we cannot continue or we will get a primary key violation error
                    // Avoid throwing an exception, or it will prevent the DB initializer to replay events synchronously
                    return;
                }

                var addressDetails = new AddressDetails { AccountId = @event.SourceId };
                list.Add(Mapper.Map(@event.Address, addressDetails));

                var aptEvent = @event.Address.Apartment ?? string.Empty;
                var ringCodeEvent = @event.Address.RingCode ?? string.Empty;
                var fullAddressEvent = @event.Address.FullAddress;

                var identicalHistoricAddress =
                    (from a in list.Where(x => x.IsHistoric)
                     where (a.Apartment ?? string.Empty) == aptEvent
                     where a.FullAddress == fullAddressEvent
                     where (a.RingCode ?? string.Empty) == ringCodeEvent
                     select a).FirstOrDefault();

                if (identicalHistoricAddress != null)
                {
                    list.Remove(identicalHistoricAddress);
                }

            });
            
        }

        public void Handle(FavoriteAddressRemoved @event)
        {
            _addressDetailProjectionSet.Update(@event.SourceId, list =>
            {
                var address = list.SingleOrDefault(x => x.Id == @event.AddressId);
                if (address != null && !address.IsHistoric)
                {
                    list.Remove(address);
                }
            });
        }

        public void Handle(FavoriteAddressUpdated @event)
        {
            _addressDetailProjectionSet.Update(@event.SourceId, list =>
            {
                var address = list.SingleOrDefault(x => x.Id == @event.Address.Id);
                if (address != null)
                {
                    address.IsHistoric = false;
                    Mapper.Map(@event.Address, address);
                }
            });
        }

        public void Handle(OrderCreated @event)
        {
            _addressDetailProjectionSet.Update(@event.AccountId, list =>
            {
                var identicalAddresses = from a in list
                    where (a.Apartment ?? string.Empty) == (@event.PickupAddress.Apartment ?? string.Empty)
                    where a.FullAddress == @event.PickupAddress.FullAddress
                    where (a.RingCode ?? string.Empty) == (@event.PickupAddress.RingCode ?? string.Empty)
// ReSharper disable once CompareOfFloatsByEqualityOperator
                    where a.Latitude == @event.PickupAddress.Latitude
// ReSharper disable once CompareOfFloatsByEqualityOperator
                    where a.Longitude == @event.PickupAddress.Longitude
                    select a;

                if (!identicalAddresses.Any())
                {
                    var address = new AddressDetails();
                    Mapper.Map(@event.PickupAddress, address);
                    address.Id = Guid.NewGuid();
                    address.AccountId = @event.AccountId;
                    address.IsHistoric = true;
                    list.Add(address);
                }
            });
        }

        public void Handle(PopularAddressAdded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = new PopularAddressDetails();
                Mapper.Map(@event.Address, address);
                context.Save(address);
            }
        }

        public void Handle(PopularAddressRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<PopularAddressDetails>(@event.AddressId);
                if (address != null)
                {
                    context.Set<PopularAddressDetails>().Remove(address);
                    context.SaveChanges();
                }
            }
        }

        public void Handle(PopularAddressUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<PopularAddressDetails>(@event.Address.Id);
                if (address != null)
                {
                    Mapper.Map(@event.Address, address);
                    context.SaveChanges();
                }
            }
        }

        public void Handle(AccountRegistered @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                //TODO remove this 
                var defaultCompanyAddresses = context.Query<DefaultAddressDetails>().ToList();

                //add default company favorite addressed
                var addresses = defaultCompanyAddresses.Select(c => new AddressDetails
                {
                    AccountId = @event.SourceId,
                    Apartment = c.Apartment,
                    BuildingName = c.BuildingName,
                    FriendlyName = c.FriendlyName,
                    FullAddress = c.FullAddress,
                    Id = Guid.NewGuid(),
                    IsHistoric = false,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    RingCode = c.RingCode
                });

                _addressDetailProjectionSet.Add(new AddressDetailCollection(@event.SourceId, addresses));
            }
        }
    }

    public abstract class AddressDetailProjectionSet : IProjectionSet<AddressDetailCollection>
    {
        public abstract void Add(AddressDetailCollection projection);

        public abstract void AddOrReplace(AddressDetailCollection projection);

        public abstract void AddRange(IEnumerable<AddressDetailCollection> projections);

        public abstract bool Exists(Guid identifier);

        public abstract bool Exists(Func<AddressDetailCollection, bool> predicate);

        public abstract void Remove(Guid identifier);

        public abstract IProjection<AddressDetailCollection> GetProjection(Guid identifier);

        public abstract void Update(Func<AddressDetailCollection, bool> predicate, Action<AddressDetailCollection> action);

        public abstract void Update(Guid identifier, Action<AddressDetailCollection> action);
    }

    public class AddressDetailMemoryProjectionSet : AddressDetailProjectionSet, IEnumerable<AddressDetailCollection>
    {
        readonly IDictionary<Guid, AddressDetailCollection> _cache = new Dictionary<Guid, AddressDetailCollection>();

        public override void Add(AddressDetailCollection projection)
        {
            if(projection == null)
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
                context.Set<AddressDetails>().AddRange(projections.SelectMany(x => x));
                context.SaveChanges();
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
            :this(identifier, Enumerable.Empty<AddressDetails>())
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
            if(item == null)
            {
                throw new ArgumentNullException();
            }
            if(item.AccountId != Identifier)
            {
                throw new InvalidOperationException();
            }
            if(_innerList.Any(x => x.Id == item.Id))
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