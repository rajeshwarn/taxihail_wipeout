#region

using System;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderPaymentManager :
        IIntegrationEventHandler,
        IEventHandler<PayPalExpressCheckoutPaymentCompleted>,
        IEventHandler<CreditCardPaymentCaptured>
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IOrderDao _dao;
        private readonly IIbsOrderService _ibs;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IAccountDao _accountDao;
        public OrderPaymentManager(IOrderDao dao, IOrderPaymentDao paymentDao, IAccountDao accountDao, ICreditCardDao creditCardDao, IIbsOrderService ibs, IConfigurationManager configurationManager)
        {
            _accountDao = accountDao;
            _dao = dao;
            _paymentDao = paymentDao;
            _creditCardDao = creditCardDao;
            _ibs = ibs;
            _configurationManager = configurationManager;
        }

        public void Handle(PayPalExpressCheckoutPaymentCompleted @event)
        {
            // Send message to driver
            SendPaymentConfirmationToDriver(@event.OrderId, @event.Amount, 0, @event.Amount, PaymentType.PayPal.ToString(), PaymentProvider.PayPal.ToString(), @event.TransactionId, @event.PayPalPayerId);

        }

        public void Handle(CreditCardPaymentCaptured @event)
        {
            SendPaymentConfirmationToDriver(@event.OrderId, @event.Amount, @event.Tip, @event.Meter, PaymentType.CreditCard.ToString(), @event.Provider.ToString(), @event.TransactionId, @event.AuthorizationCode);
        }

        private void SendPaymentConfirmationToDriver(Guid orderId, decimal amount, decimal tip, decimal meter, string type, string provider, string transactionId, string authorizationCode)
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

            

            string lastForDigit = "";

            if ( provider == PaymentType.CreditCard.ToString () )
            {
                var card = _creditCardDao.FindByToken(payment.CardToken);
                if (card == null) throw new InvalidOperationException("Credit card not found");
                lastForDigit = card.Last4Digits;                
            }
                       

            var applicationKey = _configurationManager.GetSetting("TaxiHail.ApplicationKey");
            var resources = new DynamicResources(applicationKey);

            var line1 = string.Format(resources.GetString("PaymentConfirmationToDriver1"), amount);
            line1 = line1.PadRight(32, ' ');
            //Padded with 32 char because the MDT displays line of 32 char.  This will cause to write the auth code on the second line
            var line2 = string.Format(resources.GetString("PaymentConfirmationToDriver2"), authorizationCode);
            
            _ibs.SendPaymentNotification(line1 + line2, orderStatusDetail.VehicleNumber, orderDetail.IBSOrderId.Value);
            
            _ibs.ConfirmExternalPayment(orderDetail.IBSOrderId.Value, amount, tip, meter, type, provider, transactionId, authorizationCode, payment.CardToken, account.IBSAccountId, orderDetail.Settings.Name, orderDetail.Settings.Phone, account.Email, orderDetail.UserAgent.GetOperatingSystem(), orderDetail.UserAgent);
            
        }
    }
}