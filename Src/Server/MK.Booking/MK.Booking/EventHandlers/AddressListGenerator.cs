#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using AutoMapper;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class AddressListGenerator : IEventHandler<FavoriteAddressAdded>, IEventHandler<FavoriteAddressRemoved>,
        IEventHandler<FavoriteAddressUpdated>, IEventHandler<OrderCreated>, IEventHandler<AddressRemovedFromHistory>,
        IEventHandler<DefaultFavoriteAddressAdded>, IEventHandler<DefaultFavoriteAddressRemoved>,
        IEventHandler<DefaultFavoriteAddressUpdated>
        , IEventHandler<PopularAddressAdded>, IEventHandler<PopularAddressRemoved>, IEventHandler<PopularAddressUpdated>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public AddressListGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(AddressRemovedFromHistory @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<AddressDetails>(@event.AddressId);
                if (address != null)
                {
                    context.Set<AddressDetails>().Remove(address);
                    context.SaveChanges();
                }
            }
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
            using (var context = _contextFactory.Invoke())
            {
                var existingAddress = context.Find<AddressDetails>(@event.Address.Id);
                if (existingAddress != null)
                {
                    // TODO: Log this problem
                    // Address already exist, we cannot continue or we will get a primary key violation error
                    // Avoid throwing an exception, or it will prevent the DB initializer to replay events synchronously
                    return;
                }

                var addressDetails = new AddressDetails {AccountId = @event.SourceId};
                Mapper.Map(@event.Address, addressDetails);
                context.Save(addressDetails);

                if (@event.Address != null)
                {
                    var aptEvent = @event.Address.Apartment ?? string.Empty;
                    var ringCodeEvent = @event.Address.RingCode ?? string.Empty;
                    var fullAddressEvent = @event.Address.FullAddress;
                    var identicalHistoricAddress =
                        (from a in context.Query<AddressDetails>().Where(x => x.IsHistoric)
                            where a.AccountId == @event.SourceId
                            where (a.Apartment ?? string.Empty) == aptEvent
                            where a.FullAddress == fullAddressEvent
                            where (a.RingCode ?? string.Empty) == ringCodeEvent
                            select a).FirstOrDefault();

                    if (identicalHistoricAddress != null)
                    {
                        context.Set<AddressDetails>().Remove(identicalHistoricAddress);
                        context.SaveChanges();
                    }
                }
            }
        }

        public void Handle(FavoriteAddressRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<AddressDetails>(@event.AddressId);
                if (address != null && !address.IsHistoric)
                {
                    context.Set<AddressDetails>().Remove(address);
                    context.SaveChanges();
                }
            }
        }

        public void Handle(FavoriteAddressUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<AddressDetails>(@event.Address.Id);
                if (address != null)
                {
                    address.IsHistoric = false;
                    Mapper.Map(@event.Address, address);
                    context.SaveChanges();
                }
            }
        }

        public void Handle(OrderCreated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var identicalAddresses = from a in context.Query<AddressDetails>()
                    where a.AccountId == @event.AccountId
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
                    context.Save(address);
                }
            }
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
    }
}