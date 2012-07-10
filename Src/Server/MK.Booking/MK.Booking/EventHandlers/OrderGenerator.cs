using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class OrderGenerator : IEventHandler<OrderCreated>, IEventHandler<OrderCancelled>
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
                    PickupApartment = @event.PickupApartment,
                    PickupAddress = @event.PickupAddress,
                    PickupLatitude = @event.PickupLatitude,
                    Id = @event.SourceId,
                    PickupLongitude = @event.PickupLongitude,
                    PickupDate = @event.PickupDate,
                    PickupRingCode = @event.PickupRingCode,
                    RequestedDateTime = @event.RequestedDate,
                    DropOffAddress = @event.DropOffAddress,
                    DropOffLatitude = @event.DropOffLatitude,
                    DropOffLongitude = @event.DropOffLongitude,
                    Status = @event.Status,
                });
            }
        }

        public void Handle(OrderCancelled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var order = context.Find<OrderDetail>(@event.SourceId);

                //TODO update order statuts here

                context.Save(order);
            }
        }
    }
}
