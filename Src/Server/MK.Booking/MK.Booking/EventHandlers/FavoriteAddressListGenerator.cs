using System;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.BackOffice.EventHandlers
{
    public class FavoriteAddressListGenerator : IEventHandler<FavoriteAddressAdded>, IEventHandler<FavoriteAddressRemoved>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        public FavoriteAddressListGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;

        }

        public void Handle(FavoriteAddressAdded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new FavoriteAddress
                {
                    AccountId = @event.SourceId,
                    FriendlyName = @event.FriendlyName,
                    Apartment = @event.Apartment,
                    FullAddress = @event.FullAddress,
                    RingCode = @event.RingCode,
                    Latitude = @event.Latitude,
                    Longitude = @event.Longitude
                });
            }
        }

        public void Handle(FavoriteAddressRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<FavoriteAddress>(@event.AddressId);
                context.Set<FavoriteAddress>().Remove(address);
                context.SaveChanges();
            }
        }
    }
}
