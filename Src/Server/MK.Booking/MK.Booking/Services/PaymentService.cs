using System;
using System.Linq;
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

        public PaymentProvider ProviderType(Guid? orderId = null)
        {
            if (IsPayPal(orderId: orderId))
            {
                return PaymentProvider.PayPal;
            }

            return GetInstance().ProviderType(orderId);
        }

        public PreAuthorizePaymentResponse PreAuthorize(Guid orderId, AccountDetail account, decimal amountToPreAuthorize, bool isReAuth = false, bool isSettlingOverduePayment = false, bool isForPrepaid = false, string cvv = null)
        {
            // we pass the orderId just in case it might exist but most of the time it won't since preauth is done before order creation
            if (IsPayPal(account.Id, orderId, isForPrepaid))
            {
                return _payPalServiceFactory.GetInstance().PreAuthorize(account.Id, orderId, account.Email, amountToPreAuthorize, isReAuth);
            }

            // if we call preauth more than once, the cvv will be null but since preauth already passed once, it's safe to assume it's ok
            var response = GetInstance().PreAuthorize(orderId, account, amountToPreAuthorize, isReAuth, isSettlingOverduePayment, false, cvv);

            // TODO when CMT has preauth enabled, remove this ugly code and delete temp info for everyone
            // we can't delete here for CMT because we need the cvv info in the CommitPayment method
            var serverSettings = _container.Resolve<IServerSettings>();
            if (serverSettings.GetPaymentSettings().PaymentMode != PaymentMethod.Cmt &&
                serverSettings.GetPaymentSettings().PaymentMode != PaymentMethod.RideLinqCmt)
            {
                // delete the cvv stored in database once preauth is done, doesn't fail if it doesn't exist
                _orderDao.DeleteTemporaryPaymentInfo(orderId);
            }
            
            return response;
        }

        public CommitPreauthorizedPaymentResponse CommitPayment(Guid orderId, AccountDetail account, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId, string reAuthOrderId = null, bool isForPrepaid = false)
        {
            if (IsPayPal(orderId: orderId, isForPrepaid: isForPrepaid))
            {
                return _payPalServiceFactory.GetInstance().CommitPayment(orderId, preauthAmount, amount, meterAmount, tipAmount, transactionId);
            }

            return GetInstance().CommitPayment(orderId, account, preauthAmount, amount, meterAmount, tipAmount, transactionId, reAuthOrderId);
        }

        public BasePaymentResponse RefundPayment(Guid orderId)
        {
            if (IsPayPal(orderId: orderId))
            {
                return _payPalServiceFactory.GetInstance().RefundWebPayment(orderId);
            }

            return GetInstance().RefundPayment(orderId);
        }

        public BasePaymentResponse UpdateAutoTip(Guid orderId, int autoTipPercentage)
        {
            if (IsPayPal(orderId: orderId))
            {
                throw new NotImplementedException("Method only implemented for CMT RideLinQ");
            }

            return GetInstance().UpdateAutoTip(orderId, autoTipPercentage);
        }

        public DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(string cardToken)
        {
            if (IsPayPal())
            {
                // No CC to delete with PayPal
                return new DeleteTokenizedCreditcardResponse { IsSuccessful = true };
            }
            return GetInstance().DeleteTokenizedCreditcard(cardToken);
        }

        public PairingResponse Pair(Guid orderId,string cardToken, int autoTipPercentage)
        {
            var order = _orderDao.FindById(orderId);

            if (IsPayPal(null, order))
            {
                return _payPalServiceFactory.GetInstance().Pair(orderId, autoTipPercentage);
            }

            var card = _creditCardDao.FindByAccountId(order.AccountId).First();
            return GetInstance().Pair(orderId, card.Token, autoTipPercentage);
        }

        public BasePaymentResponse Unpair(Guid orderId)
        {
            if (IsPayPal(orderId: orderId))
            {
                return _payPalServiceFactory.GetInstance().Unpair(orderId);
            }

            return GetInstance().Unpair(orderId);
        }

        public void VoidPreAuthorization(Guid orderId, bool isForPrepaid = false)
        {
            if (IsPayPal(orderId: orderId, isForPrepaid: isForPrepaid))
            {
                _payPalServiceFactory.GetInstance().VoidPreAuthorization(orderId);
            }
            else
            {
                var paymentService = GetInstance();
                if (paymentService != null) // payment might not be enabled
                {
                    paymentService.VoidPreAuthorization(orderId);
                }
            }
        }

        public void VoidTransaction(Guid orderId, string transactionId, ref string message)
        {
            if (IsPayPal(orderId: orderId))
            {
                _payPalServiceFactory.GetInstance().VoidTransaction(orderId, transactionId, ref message);
            }
            else
            {
                GetInstance().VoidTransaction(orderId, transactionId, ref message);
            }
        }

        private IPaymentService GetInstance(Guid orderId)
        {
            var serverSettings = _container.Resolve<IServerSettings>();
            switch (serverSettings.GetPaymentSettings().PaymentMode)
            {
                case PaymentMethod.Braintree:
                    return new BraintreePaymentService(_container.Resolve<ICommandBus>(), _container.Resolve<ILogger>(), _container.Resolve<IOrderPaymentDao>(), serverSettings, _container.Resolve<IPairingService>(), _container.Resolve<ICreditCardDao>());
                case PaymentMethod.RideLinqCmt:
                case PaymentMethod.Cmt:
                    return new CmtPaymentService(_container.Resolve<ICommandBus>(), _container.Resolve<IOrderDao>(), _container.Resolve<ILogger>(), _container.Resolve<IAccountDao>(), _container.Resolve<IOrderPaymentDao>(), serverSettings, _container.Resolve<IPairingService>(), _container.Resolve<ICreditCardDao>());
                case PaymentMethod.Moneris:
                    return new MonerisPaymentService(_container.Resolve<ICommandBus>(), _container.Resolve<ILogger>(), _container.Resolve<IOrderPaymentDao>(), serverSettings, _container.Resolve<IPairingService>(), _container.Resolve<ICreditCardDao>(), _container.Resolve<IOrderDao>());
                default:
                    return null;
            }
        }
    }
}