using System;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderPairingManager:
        IIntegrationEventHandler,
        IEventHandler<OrderStatusChanged>
    {
        private readonly INotificationService _notificationService;
        private readonly IServerSettings _serverSettings;
        private readonly IOrderDao _orderDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IAccountDao _accountDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IPaymentServiceFactory _paymentServiceFactory;

        public OrderPairingManager(IPaymentServiceFactory paymentServiceFactory, 
            INotificationService notificationService, 
            IServerSettings serverSettings,
            IOrderDao orderDao,
            ICreditCardDao creditCardDao,
            IAccountDao accountDao,
            IIBSServiceProvider ibsServiceProvider)
        {
            _paymentServiceFactory = paymentServiceFactory;
            _notificationService = notificationService;
            _serverSettings = serverSettings;
            _orderDao = orderDao;
            _creditCardDao = creditCardDao;
            _accountDao = accountDao;
            _ibsServiceProvider = ibsServiceProvider;
        }

        public void Handle(OrderStatusChanged @event)
        {
            switch (@event.Status.IBSStatusId)
            {
                case VehicleStatuses.Common.Loaded:
                {
                    var order = _orderDao.FindById(@event.SourceId);
                    var creditCardAssociatedToAccount = _creditCardDao.FindByAccountId(@event.Status.AccountId).FirstOrDefault();

                    if (_serverSettings.GetPaymentSettings().AutomaticPayment
                        && _serverSettings.GetPaymentSettings().AutomaticPaymentPairing
                        && _serverSettings.GetPaymentSettings().PaymentMode != PaymentMethod.RideLinqCmt
                        && order.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
                        && creditCardAssociatedToAccount != null)        // Only send notification if using card on file
                    {
                        var account = _accountDao.FindById(@event.Status.AccountId);

                        var paymentService = _paymentServiceFactory.GetInstance();
                        var response = paymentService.Pair(@event.SourceId, creditCardAssociatedToAccount.Token, account.DefaultTipPercent, null);

                        if (response.IsSuccessful)
                        {
                            var ibsAccountId = _accountDao.GetIbsAccountId(order.AccountId, null);
                            if (!UpdateOrderPaymentType(ibsAccountId.Value, order.IBSOrderId.Value))
                            {
                                response.IsSuccessful = false;
                                paymentService.VoidPreAuthorization(@event.SourceId);
                            }
                        }

                        _notificationService.SendAutomaticPairingPush(@event.SourceId, account.DefaultTipPercent, creditCardAssociatedToAccount.Last4Digits, response.IsSuccessful);
                    } 
                }
                break;
            }
        }

        private bool UpdateOrderPaymentType(int ibsAccountId, int ibsOrderId, string companyKey = null)
        {
            return _ibsServiceProvider.Booking(companyKey).UpdateOrderPaymentType(ibsAccountId, ibsOrderId, _serverSettings.ServerData.IBS.PaymentTypeCardOnFileId);
        }
    }
}
