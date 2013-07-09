using System;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderPaymentManager:
        IIntegrationEventHandler,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>
    {
        readonly ICommandBus _commandBus;
        readonly IOrderDao _dao;
        readonly IIBSOrderService _ibs;

        public OrderPaymentManager(ICommandBus commandBus, IOrderDao dao, IIBSOrderService ibs)
        {
            _commandBus = commandBus;
            _dao = dao;
            _ibs = ibs;
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            // Confirm to IBS that order was payed
            var orderDetail = _dao.FindById(@event.OrderId);
            if (orderDetail == null) throw new InvalidOperationException("Order not found");
            if (orderDetail.IBSOrderId == null) throw new InvalidOperationException("IBSOrderId should not be null");
            
            var transactionId = string.Format("PayPal Express Checkout | Token:{0} | PayerId:{1}", @event.Token, @event.PayPalPayerId);
            _ibs.ConfirmExternalPayment(orderDetail.IBSOrderId.Value, @event.Amount, transactionId);

            // Send message to driver
            var orderStatusDetail = _dao.FindOrderStatusById(@event.OrderId);
            if (orderStatusDetail == null) throw new InvalidOperationException("Order Status not found");

            _ibs.SendMessageToDriver("The passenger has payed " + @event.Amount.ToString("C"), orderStatusDetail.VehicleNumber);
        }
    }
}
