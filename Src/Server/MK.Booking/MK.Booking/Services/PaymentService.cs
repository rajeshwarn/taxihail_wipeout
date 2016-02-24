using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Resources;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPayPalServiceFactory _payPalServiceFactory;
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IServerSettings _serverSettings;
        private readonly IUnityContainer _container;

        public PaymentService(IPayPalServiceFactory payPalServiceFactory, 
            IAccountDao accountDao, 
            IOrderDao orderDao, 
            ICreditCardDao creditCardDao,
            IServerSettings serverSettings,
            IUnityContainer container)
        {
            _payPalServiceFactory = payPalServiceFactory;
            _accountDao = accountDao;
            _orderDao = orderDao;
            _creditCardDao = creditCardDao;
            _serverSettings = serverSettings;
            _container = container;
        }

        public bool IsPayPal(Guid? accountId = null, Guid? orderId = null, bool isForPrepaid = false)
        {
            AccountDetail account = null;
            if (accountId.HasValue)
            {
                account = _accountDao.FindById(accountId.Value);
            }

            if (orderId.HasValue)
            {
                var order = _orderDao.FindById(orderId.Value);
                return IsPayPal(account, order, isForPrepaid);
            }

            return IsPayPal(account, null, isForPrepaid);
        }

        private bool IsPayPal(AccountDetail account, OrderDetail order, bool isForPrepaid = false)
        {
            if (isForPrepaid)
            {
                return false;
            }

            // check local settings because if it's not enabled locally, the user won't have a paypal account linked
            var payPalIsEnabled = GetPaymentSettings(null).PayPalClientSettings.IsEnabled;
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

        public PaymentProvider ProviderType(string companyKey, Guid? orderId = null)
        {
            if (IsPayPal(orderId: orderId))
            {
                return PaymentProvider.PayPal;
            }

            return GetInstance(companyKey).ProviderType(companyKey, orderId);
        }

        /// <summary>
        /// If we ever need to support fees or CoF network payment with other providers than CMT/RideLinq,
        /// we'll need to keep track of on which company the pre-auth has been made and handle it properly because, as of today, orderPaymentDetail is unique by order.
        /// </summary>
        public PreAuthorizePaymentResponse PreAuthorize(string companyKey, Guid orderId, AccountDetail account, decimal amountToPreAuthorize, bool isReAuth = false, bool isSettlingOverduePayment = false, bool isForPrepaid = false, string cvv = null)
        {
            // we pass the orderId just in case it might exist but most of the time it won't since preauth is done before order creation
            if (IsPayPal(account.Id, orderId, isForPrepaid))
            {
                return _payPalServiceFactory.GetInstance(companyKey).PreAuthorize(account.Id, orderId, account.Email, amountToPreAuthorize, isReAuth);
            }

            // if we call preauth more than once, the cvv will be null but since preauth already passed once, it's safe to assume it's ok
            var response = GetInstance(companyKey).PreAuthorize(companyKey, orderId, account, amountToPreAuthorize, isReAuth, isSettlingOverduePayment, false, cvv);

            // when CMT has preauth enabled, remove this ugly code and delete temp info for everyone
            // we can't delete here for CMT because we need the cvv info in the CommitPayment method
            if (GetPaymentSettings(companyKey).PaymentMode != PaymentMethod.Cmt &&
                GetPaymentSettings(companyKey).PaymentMode != PaymentMethod.RideLinqCmt)
            {
                // delete the cvv stored in database once preauth is done, doesn't fail if it doesn't exist
                _orderDao.DeleteTemporaryPaymentInfo(orderId);
            }
            
            return response;
        }

        public CommitPreauthorizedPaymentResponse CommitPayment(string companyKey, Guid orderId, AccountDetail account, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId, string reAuthOrderId = null, bool isForPrepaid = false)
        {
            if (IsPayPal(orderId: orderId, isForPrepaid: isForPrepaid))
            {
                return _payPalServiceFactory.GetInstance(companyKey).CommitPayment(companyKey, orderId, preauthAmount, amount, meterAmount, tipAmount, transactionId);
            }

            return GetInstance(companyKey).CommitPayment(companyKey, orderId, account, preauthAmount, amount, meterAmount, tipAmount, transactionId, reAuthOrderId);
        }

        public async Task<RefundPaymentResponse> RefundPayment(string companyKey, Guid orderId)
        {
            if (IsPayPal(orderId: orderId))
            {
                return _payPalServiceFactory.GetInstance(companyKey).RefundWebPayment(companyKey, orderId);
            }

            return await GetInstance(companyKey).RefundPayment(companyKey, orderId);
        }

        public Task<BasePaymentResponse> UpdateAutoTip(string companyKey, Guid orderId, int autoTipPercentage)
        {
            if (IsPayPal(orderId: orderId))
            {
                throw new NotImplementedException("Method only implemented for CMT RideLinQ");
            }

            return GetInstance(companyKey).UpdateAutoTip(companyKey, orderId, autoTipPercentage);
        }

        public DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(string cardToken)
        {
            if (IsPayPal())
            {
                // No CC to delete with PayPal
                return new DeleteTokenizedCreditcardResponse { IsSuccessful = true };
            }
            return GetInstance(null).DeleteTokenizedCreditcard(cardToken);
        }

        public async Task<PairingResponse> Pair(string companyKey, Guid orderId, string cardToken, int autoTipPercentage)
        {
            var order = _orderDao.FindById(orderId);

            if (IsPayPal(null, order))
            {
                return _payPalServiceFactory.GetInstance(companyKey).Pair(orderId, autoTipPercentage);
            }

            var account = _accountDao.FindById(order.AccountId);
            var creditCard = _creditCardDao.FindById(account.DefaultCreditCard.GetValueOrDefault());

            return await GetInstance(companyKey).Pair(companyKey, orderId, creditCard.Token, autoTipPercentage);
        }

        public async Task<BasePaymentResponse> Unpair(string companyKey, Guid orderId)
        {
            if (IsPayPal(orderId: orderId))
            {
                return _payPalServiceFactory.GetInstance(companyKey).Unpair(orderId);
            }

            return await GetInstance(companyKey).Unpair(companyKey, orderId);
        }

        public void VoidPreAuthorization(string companyKey, Guid orderId, bool isForPrepaid = false)
        {
            if (IsPayPal(orderId: orderId, isForPrepaid: isForPrepaid))
            {
                _payPalServiceFactory.GetInstance(companyKey).VoidPreAuthorization(companyKey, orderId);
            }
            else
            {
                var paymentService = GetInstance(companyKey);
                if (paymentService != null) // payment might not be enabled
                {
                    paymentService.VoidPreAuthorization(companyKey, orderId);
                }
            }
        }

        public void VoidTransaction(string companyKey, Guid orderId, string transactionId, ref string message)
        {
            if (IsPayPal(orderId: orderId))
            {
                _payPalServiceFactory.GetInstance(companyKey).VoidTransaction(companyKey, orderId, transactionId, ref message);
            }
            else
            {
                GetInstance(companyKey).VoidTransaction(companyKey, orderId, transactionId, ref message);
            }
        }

        private ServerPaymentSettings GetPaymentSettings(string companyKey)
        {
            return _serverSettings.GetPaymentSettings(companyKey);
        }

        private IPaymentService GetInstance(string companyKey)
        {
            var paymentSettings = GetPaymentSettings(companyKey);

            switch (paymentSettings.PaymentMode)
            {
                case PaymentMethod.Braintree:
                    return new BraintreePaymentService(_container.Resolve<ICommandBus>(), _container.Resolve<ILogger>(), _container.Resolve<IOrderPaymentDao>(), _container.Resolve<IOrderDao>(), _serverSettings, paymentSettings, _container.Resolve<IPairingService>(), _container.Resolve<ICreditCardDao>());
                case PaymentMethod.RideLinqCmt:
                case PaymentMethod.Cmt:
                    return new CmtPaymentService(_container.Resolve<ICommandBus>(), _container.Resolve<IOrderDao>(), _container.Resolve<ILogger>(), _container.Resolve<IAccountDao>(), _container.Resolve<IOrderPaymentDao>(), paymentSettings, _container.Resolve<IPairingService>(), _container.Resolve<ICreditCardDao>());
                case PaymentMethod.Moneris:
                    return new MonerisPaymentService(_container.Resolve<ICommandBus>(), _container.Resolve<ILogger>(), _container.Resolve<IOrderPaymentDao>(), _serverSettings, paymentSettings, _container.Resolve<IPairingService>(), _container.Resolve<ICreditCardDao>(), _container.Resolve<IOrderDao>());
                default:
                    return null;
            }
        }
    }
}