using System;
using System.Globalization;
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
            // Process payment with paypal
            var cultureName = _configurationManager.GetSetting("PriceFormat");
            var paymentSettings = (ServerPaymentSettings)_configurationManager.GetPaymentSettings();
            var payPalCredentials = paymentSettings.PayPalServerSettings.IsSandbox 
                ? paymentSettings.PayPalServerSettings.SandboxCredentials
                : paymentSettings.PayPalServerSettings.Credentials;

            var payPalService = new ExpressCheckoutServiceClient(payPalCredentials, new RegionInfo(cultureName), paymentSettings.PayPalServerSettings.IsSandbox);
            var transactionId = payPalService.DoExpressCheckoutPayment(@event.Token, @event.PayPalPayerId, @event.Amount);

            // Send message to driver
            var orderStatusDetail = _dao.FindOrderStatusById(@event.OrderId);
            if (orderStatusDetail == null) throw new InvalidOperationException("Order Status not found");

            _ibs.SendMessageToDriver("The passenger has paid " + @event.Amount.ToString("C"), orderStatusDetail.VehicleNumber);

            // Confirm to IBS that order was payed
            var orderDetail = _dao.FindById(@event.OrderId);
            if (orderDetail == null) throw new InvalidOperationException("Order not found");
            if (orderDetail.IBSOrderId == null) throw new InvalidOperationException("IBSOrderId should not be null");

            _ibs.ConfirmExternalPayment(orderDetail.IBSOrderId.Value, @event.Amount, transactionId);

        }

        public void Handle(CreditCardPaymentCaptured @event)
        {
            // Send message to driver
            var orderStatusDetail = _dao.FindOrderStatusById(@event.OrderId);
            if (orderStatusDetail == null) throw new InvalidOperationException("Order Status not found");

            _ibs.SendMessageToDriver("The passenger has paid " + @event.Amount.ToString("C"), orderStatusDetail.VehicleNumber);

            // Confirm to IBS that order was payed
            var orderDetail = _dao.FindById(@event.OrderId);
            if (orderDetail == null) throw new InvalidOperationException("Order not found");
            if (orderDetail.IBSOrderId == null) throw new InvalidOperationException("IBSOrderId should not be null");

            _ibs.ConfirmExternalPayment(orderDetail.IBSOrderId.Value, @event.Amount, @event.TransactionId);
        }
    }
}
