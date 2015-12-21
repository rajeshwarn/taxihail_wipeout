using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using AutoMapper;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Projections;

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
}