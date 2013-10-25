using System;
using System.Globalization;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;
using MK.Booking.PayPal;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderPaymentManager:
        IIntegrationEventHandler,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>,
        IEventHandler<CreditCardPaymentCaptured>
    {
        readonly IOrderDao _dao;
        readonly IIBSOrderService _ibs;
        readonly IConfigurationManager _configurationManager;

        public OrderPaymentManager(IOrderDao dao, IIBSOrderService ibs, IConfigurationManager configurationManager)
        {
            _dao = dao;
            _ibs = ibs;
            _configurationManager = configurationManager;
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            // Send message to driver
            SendPaymentConfirmationToDriver(@event.OrderId, @event.Amount, PaymentType.PayPal.ToString(), PaymentProvider.PayPal.ToString(), @event.TransactionId, @event.PayPalPayerId);

        }

        public void Handle(CreditCardPaymentCaptured @event)
        {
            SendPaymentConfirmationToDriver(@event.OrderId, @event.Amount, PaymentType.CreditCard.ToString(), @event.Provider.ToString(),  @event.TransactionId, @event.AuthorizationCode);
        }

        private void SendPaymentConfirmationToDriver(Guid orderId, decimal amount,  string type, string provider, string transactionId, string authorizationCode)
        {
            // Send message to driver
            var orderStatusDetail = _dao.FindOrderStatusById(orderId);
            if (orderStatusDetail == null) throw new InvalidOperationException("Order Status not found");

            var applicationKey = _configurationManager.GetSetting("TaxiHail.ApplicationKey");
            var resources = new DynamicResources(applicationKey);

            _ibs.SendMessageToDriver( string.Format( resources.GetString( "PaymentConfirmationToDriver"), amount, authorizationCode ), orderStatusDetail.VehicleNumber);

            // Confirm to IBS that order was payed
            var orderDetail = _dao.FindById(orderId);
            if (orderDetail == null) throw new InvalidOperationException("Order not found");
            if (orderDetail.IBSOrderId == null) throw new InvalidOperationException("IBSOrderId should not be null");

            _ibs.ConfirmExternalPayment(orderDetail.IBSOrderId.Value, amount, type, provider , transactionId, authorizationCode);
        }
    }
}
