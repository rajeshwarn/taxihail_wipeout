using System;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Events;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderPaymentManager:
        IIntegrationEventHandler,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>
    {
        private readonly ICommandBus _commandBus;

        public OrderPaymentManager(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            _commandBus.Send(new MarkOrderAsPayed
            {
                OrderId = @event.OrderId,                        
            });
        }
    }

    public class MarkOrderAsPayed : ICommand
    {
        public MarkOrderAsPayed()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; private set; }
        public Guid OrderId { get; set; }
    }
}
