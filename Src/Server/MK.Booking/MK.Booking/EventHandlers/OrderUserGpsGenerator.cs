using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Diagnostic;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class OrderUserGpsGenerator : IEventHandler<OrderCreated>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ILogger _logger;

        public OrderUserGpsGenerator(Func<BookingDbContext> contextFactory, ILogger logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public void Handle(OrderCreated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var info = new OrderUserGpsDetail
                {
                    OrderId = @event.SourceId,
                    UserLatitude = @event.UserLatitude,
                    UserLongitude = @event.UserLongitude
                };
                context.Save(info);
            }
        }
    }
}