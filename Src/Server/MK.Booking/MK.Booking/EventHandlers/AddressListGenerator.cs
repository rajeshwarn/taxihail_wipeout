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
                context.Save(new AddressDetails
                {
                    Id = @event.AddressId,
                    AccountId = @event.SourceId,
                    FriendlyName = @event.FriendlyName,
                    Apartment = @event.Apartment,
                    FullAddress = @event.FullAddress,
                    RingCode = @event.RingCode,
                    BuildingName = @event.BuildingName,
                    Latitude = @event.Latitude,
                    Longitude = @event.Longitude,
                    IsHistoric = false
                });

                var identicalHistoricAddress = (from a in context.Query<AddressDetails>().Where(x=>x.IsHistoric)
                                    where a.AccountId == @event.SourceId
                                    where (a.Apartment ?? string.Empty) == (@event.Apartment ?? string.Empty)
                                    where a.FullAddress == @event.FullAddress
                                    where (a.RingCode ?? string.Empty) == (@event.RingCode ?? string.Empty)
                                    select a).FirstOrDefault();

                if (identicalHistoricAddress != null)
                {
                    context.Set<AddressDetails>().Remove(identicalHistoricAddress);
                    context.SaveChanges();
                }
               
            }
        }

        public void Handle(FavoriteAddressRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<AddressDetails>(@event.AddressId);
                if (!address.IsHistoric)
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
                var address = context.Find<AddressDetails>(@event.AddressId);
                address.IsHistoric = false;
                AutoMapper.Mapper.Map(@event, address);
                context.SaveChanges();
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
                                         where a.Latitude == @event.PickupAddress.Latitude
                                         where a.Longitude == @event.PickupAddress.Longitude
                                         select a;

                if (!identicalAddresses.Any())
                {
                    var address = new AddressDetails();
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
                var address = context.Find<AddressDetails>(@event.AddressId);
                context.Set<AddressDetails>().Remove(address);
                context.SaveChanges();
            }
        }
    }
}
