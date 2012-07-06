using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class OrderGenerator : IEventHandler<OrderCreated>
    {

        private readonly Func<BookingDbContext> _contextFactory;

        public OrderGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(OrderCreated @event)
        {
            using (var context = _contextFactory.Invoke())
            {

                context.Save(new OrderDetail
                {
                    AccountId = @event.AccountId,
                    Apartment = @event.Apartment,
                    FriendlyName = @event.FriendlyName,
                    FullAddress = @event.FullAddress,
                    Latitude = @event.Latitude,
                    Id = @event.SourceId,
                    Longitude = @event.Longitude,
                    PickupDate = @event.PickupDate,
                    RequestedDateTime = @event.RequestedDateTime,
                    RingCode = @event.RingCode
                });

            }
        }
    }
}
