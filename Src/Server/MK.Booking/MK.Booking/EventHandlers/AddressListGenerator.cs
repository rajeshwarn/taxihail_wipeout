using System;
using System.Linq;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.BackOffice.EventHandlers
{
    public class AddressListGenerator : IEventHandler<FavoriteAddressAdded>, IEventHandler<FavoriteAddressRemoved>, IEventHandler<FavoriteAddressUpdated>, IEventHandler<OrderCreated>, IEventHandler<AddressRemovedFromHistory>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        public AddressListGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(FavoriteAddressAdded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new Address
                {
                    Id = @event.AddressId,
                    AccountId = @event.SourceId,
                    FriendlyName = @event.FriendlyName,
                    Apartment = @event.Apartment,
                    FullAddress = @event.FullAddress,
                    RingCode = @event.RingCode,
                    Latitude = @event.Latitude,
                    Longitude = @event.Longitude,
                    IsHistoric = false
                });

                var identicalHistoricAddress = (from a in context.Query<Address>().Where(x=>x.IsHistoric)
                                    where a.AccountId == @event.SourceId
                                    where (a.Apartment ?? string.Empty) == (@event.Apartment ?? string.Empty)
                                    where a.FullAddress == @event.FullAddress
                                    where (a.RingCode ?? string.Empty) == (@event.RingCode ?? string.Empty)
                                    select a).FirstOrDefault();

                if (identicalHistoricAddress != null)
                {
                    context.Set<Address>().Remove(identicalHistoricAddress);
                    context.SaveChanges();
                }
               
            }
        }

        public void Handle(FavoriteAddressRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<Address>(@event.AddressId);
                if (!address.IsHistoric)
                {
                    context.Set<Address>().Remove(address);
                    context.SaveChanges();
                }
            }
        }

        public void Handle(FavoriteAddressUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<Address>(@event.AddressId);
                address.IsHistoric = false;
                AutoMapper.Mapper.Map(@event, address);
                context.SaveChanges();
            }
        }

        public void Handle(OrderCreated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var identicalAddresses = from a in context.Query<Address>()
                                         where a.AccountId == @event.AccountId
                                         where (a.Apartment ?? string.Empty) == (@event.PickupApartment ?? string.Empty)
                                         where a.FullAddress == @event.PickupAddress
                                         where (a.RingCode ?? string.Empty) == (@event.PickupRingCode ?? string.Empty)
                                         where a.Latitude == @event.PickupLatitude
                                         where a.Longitude == @event.PickupLongitude
                                         select a;

                if (!identicalAddresses.Any())
                {
                    var address = new Address();
                    AutoMapper.Mapper.Map(@event, address);
                    address.Id = Guid.NewGuid();
                    address.IsHistoric = true;
                    context.Save(address);
                }
            }
        }

        public void Handle(AddressRemovedFromHistory @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<Address>(@event.AddressId);
                context.Set<Address>().Remove(address);
                context.SaveChanges();
            }
        }
    }
}
