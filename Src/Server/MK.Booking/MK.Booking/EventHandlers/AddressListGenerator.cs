using System;
using System.Linq;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.BackOffice.EventHandlers
{
    public class AddressListGenerator : IEventHandler<AddressAdded>, IEventHandler<AddressRemoved>, IEventHandler<AddressUpdated>, IEventHandler<OrderCreated>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        public AddressListGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(AddressAdded @event)
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
                    IsHistoric = @event.IsHistoric
                });

                if (!@event.IsHistoric)
                {
                     var identicalAddresses = (from a in context.Query<Address>()
                                         where a.AccountId == @event.SourceId
                                         where a.Apartment == @event.Apartment
                                         where a.FullAddress == @event.FullAddress
                                         where a.RingCode == @event.RingCode
                                         where a.IsHistoric
                                         select a).FirstOrDefault();

                    if (identicalAddresses!=null)
                    {
                        //var historicAddress = context.Query<Address>().FirstOrDefault(c => c.AccountId.Equals(@event.SourceId) && c.IsHistoric.Equals(true));
                        context.Set<Address>().Remove(identicalAddresses);
                        context.SaveChanges();
                    }
                }
               
            }
        }

        public void Handle(AddressRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<Address>(@event.AddressId);
                context.Set<Address>().Remove(address);
                context.SaveChanges();
            }
        }

        public void Handle(AddressUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<Address>(@event.AddressId);
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
                                         where a.Apartment == @event.PickupApartment
                                         where a.FullAddress == @event.PickupAddress
                                         where a.RingCode == @event.PickupRingCode
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
    }
}
