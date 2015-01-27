using System;
using System.Linq;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Services
{
    public class PaymentAbstrationService : IPaymentAbstractionService
    {
        private readonly IPayPalServiceFactory _payPalServiceFactory;
        private readonly IPaymentServiceFactory _paymentServiceFactory;
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IServerSettings _serverSettings;

        public PaymentAbstrationService(IPayPalServiceFactory payPalServiceFactory, IPaymentServiceFactory paymentServiceFactory, 
            IAccountDao accountDao, IOrderDao orderDao, ICreditCardDao creditCardDao, IServerSettings serverSettings)
        {
            _payPalServiceFactory = payPalServiceFactory;
            _paymentServiceFactory = paymentServiceFactory;
            _accountDao = accountDao;
            _orderDao = orderDao;
            _creditCardDao = creditCardDao;
            _serverSettings = serverSettings;
        }

        private bool IsPayPal(Guid? accountId = null, Guid? orderId = null)
        {
            AccountDetail account = null;
            if (accountId.HasValue)
            {
                account = _accountDao.FindById(accountId.Value);
            }

            if (orderId.HasValue)
            {
                var order = _orderDao.FindById(orderId.Value);
                return IsPayPal(account, order);
            }

            return IsPayPal(account, null);
        }

        private bool IsPayPal(AccountDetail account, OrderDetail order)
        {
            var payPalIsEnabled = _serverSettings.GetPaymentSettings().PayPalClientSettings.IsEnabled;
            if (!payPalIsEnabled 
                || (account == null && order == null))
            {
                return payPalIsEnabled;
            }

            if (order != null)
            {
                return order.Settings.ChargeTypeId == ChargeTypes.PayPal.Id;
            }

            return account.IsPayPalAccountLinked;
        }

        public PaymentProvider ProviderType(Guid orderId)
        {
            if (IsPayPal(orderId: orderId))
            {
                return PaymentProvider.PayPal;
            }

            return _paymentServiceFactory.GetInstance().ProviderType;
        }

        public PreAuthorizePaymentResponse PreAuthorize(Guid orderId, AccountDetail account, decimal amountToPreAuthorize)
        {
            // we pass the orderId just in case it might exist but most of the time it won't since preauth is done before order creation
            if (IsPayPal(account.Id, orderId))
            {
                return _payPalServiceFactory.GetInstance().PreAuthorize(account.Id, orderId, account.Email, amountToPreAuthorize);
            }
            
            var card = _creditCardDao.FindByAccountId(account.Id).First();
            return _paymentServiceFactory.GetInstance().PreAuthorize(orderId, account.Email, card.Token, amountToPreAuthorize);
        }

        public CommitPreauthorizedPaymentResponse CommitPayment(Guid orderId, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId)
        {
            if (IsPayPal(orderId: orderId))
            {
                return _payPalServiceFactory.GetInstance().CommitPayment(orderId, amount, meterAmount, tipAmount, transactionId);
            }

            return _paymentServiceFactory.GetInstance().CommitPayment(orderId, amount, meterAmount, tipAmount, transactionId);
        }

        public PairingResponse Pair(Guid orderId, int? autoTipPercentage)
        {
            var order = _orderDao.FindById(orderId);

            if (IsPayPal(null, order))
            {
                return _payPalServiceFactory.GetInstance().Pair(orderId, autoTipPercentage);
            }

            var card = _creditCardDao.FindByAccountId(order.AccountId).First();
            return _paymentServiceFactory.GetInstance().Pair(orderId, card.Token, autoTipPercentage);
        }

        public BasePaymentResponse Unpair(Guid orderId)
        {
            if (IsPayPal(orderId: orderId))
            {
                return _payPalServiceFactory.GetInstance().Unpair(orderId);
            }

            return _paymentServiceFactory.GetInstance().Unpair(orderId);
        }

        public void VoidPreAuthorization(Guid orderId)
        {
            if (IsPayPal(orderId: orderId))
            {
                _payPalServiceFactory.GetInstance().VoidPreAuthorization(orderId);
            }

            var paymentService = _paymentServiceFactory.GetInstance();
            if (paymentService != null) // payment might not be enabled
            {
                paymentService.VoidPreAuthorization(orderId);
            }
        }

        public void VoidTransaction(Guid orderId, string transactionId, ref string message)
        {
            if (IsPayPal(orderId: orderId))
            {
                _payPalServiceFactory.GetInstance().VoidTransaction(orderId, transactionId, ref message);
            }

            _paymentServiceFactory.GetInstance().VoidTransaction(orderId, transactionId, ref message);
        }
    }
}