using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Events;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class PushNotificationSender 
        : IIntegrationEventHandler,
          IEventHandler<OrderStatusChanged>
    {
        public void Handle(OrderStatusChanged @event)
        {
            
        }
    }
}
