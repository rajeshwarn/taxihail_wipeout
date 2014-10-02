#region

using System;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderPaymentManager :
        IIntegrationEventHandler,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>,
        IEventHandler<CreditCardPaymentCaptured>
    {
        private readonly IOrderDao _dao;
        private readonly IIbsOrderService _ibs;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IAccountDao _accountDao;
        private readonly Resources.Resources _resources;

        public OrderPaymentManager(IOrderDao dao, IOrderPaymentDao paymentDao, IAccountDao accountDao, ICreditCardDao creditCardDao, IIbsOrderService ibs, IConfigurationManager configurationManager, IAppSettings appSettings)
        {
            _accountDao = accountDao;
            _dao = dao;
            _paymentDao = paymentDao;
            _creditCardDao = creditCardDao;
            _ibs = ibs;

            _resources = new Resources.Resources(configurationManager.ServerData.TaxiHail.ApplicationKey, appSettings);
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            // Send message to driver
            SendPaymentConfirmationToDriver(@event.OrderId, @event.Amount, @event.Meter, @event.Tip, PaymentProvider.PayPal.ToString(), @event.PayPalPayerId);
        }

        public void Handle(CreditCardPaymentCaptured @event)
        {
            SendPaymentConfirmationToDriver(@event.OrderId, @event.Amount, @event.Meter, @event.Tip, @event.Provider.ToString(), @event.AuthorizationCode);
        }

        private void SendPaymentConfirmationToDriver(Guid orderId, decimal amount, decimal meter, decimal tip, string provider,  string authorizationCode)
        {
            // Send message to driver
            var orderStatusDetail = _dao.FindOrderStatusById(orderId);
            if (orderStatusDetail == null) throw new InvalidOperationException("Order Status not found");

            // Confirm to IBS that order was payed
            var orderDetail = _dao.FindById(orderId);
            if (orderDetail == null) throw new InvalidOperationException("Order not found");
            if (orderDetail.IBSOrderId == null) throw new InvalidOperationException("IBSOrderId should not be null");

            var payment = _paymentDao.FindByOrderId(orderId);
            if (payment == null) throw new InvalidOperationException("Payment info not found");

            var account = _accountDao.FindById(orderDetail.AccountId);
            if (account == null) throw new InvalidOperationException("Order account not found");

            if ( provider == PaymentType.CreditCard.ToString () )
            {
                var card = _creditCardDao.FindByToken(payment.CardToken);
                if (card == null) throw new InvalidOperationException("Credit card not found");
            }

            var amountString = _resources.FormatPrice((double)amount);
            var meterString = _resources.FormatPrice((double?)meter);
            var tipString = _resources.FormatPrice((double?)tip);

            // Padded with 32 char because the MDT displays line of 32 char.  This will cause to write each string on a new line
            var line1 = string.Format(_resources.Get("PaymentConfirmationToDriver1"));
            line1 = line1.PadRight(32, ' ');
            var line2 = string.Format(_resources.Get("PaymentConfirmationToDriver2"), meterString, tipString);
            line2 = line2.PadRight(32, ' ');
            var line3 = string.Format(_resources.Get("PaymentConfirmationToDriver3"), amountString);
            line3 = line3.PadRight(32, ' ');
            var line4 = string.Format(_resources.Get("PaymentConfirmationToDriver4"), authorizationCode);
            
            _ibs.SendPaymentNotification(line1 + line2 + line3 + line4, orderStatusDetail.VehicleNumber, orderDetail.IBSOrderId.Value);
        }
    }
}