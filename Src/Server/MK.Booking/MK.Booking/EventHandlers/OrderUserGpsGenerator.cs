using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class OrderUserGpsGenerator : IEventHandler<OrderCreated>
    {
        private readonly IProjectionSet<OrderUserGpsDetail> _orderUserPositionProjectionSet;

        public OrderUserGpsGenerator(IProjectionSet<OrderUserGpsDetail> orderUserPositionProjectionSet)
        {
            _orderUserPositionProjectionSet = orderUserPositionProjectionSet;
        }

        public void Handle(OrderCreated @event)
        {
            _orderUserPositionProjectionSet.Add(new OrderUserGpsDetail
            {
                OrderId = @event.SourceId,
                UserLatitude = @event.UserLatitude,
                UserLongitude = @event.UserLongitude
            });
        }
    }
}