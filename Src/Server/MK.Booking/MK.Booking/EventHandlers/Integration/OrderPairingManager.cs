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
        private readonly IPaymentFacadeService _paymentFacadeService;

        public OrderPairingManager(INotificationService notificationService, 
            IServerSettings serverSettings,
            IOrderDao orderDao,
            ICreditCardDao creditCardDao,
            IAccountDao accountDao,
            IIBSServiceProvider ibsServiceProvider,
            IPaymentFacadeService paymentFacadeService)
        {
            _notificationService = notificationService;
            _serverSettings = serverSettings;
            _orderDao = orderDao;
            _creditCardDao = creditCardDao;
            _accountDao = accountDao;
            _ibsServiceProvider = ibsServiceProvider;
            _paymentFacadeService = paymentFacadeService;
        }

        public void Handle(OrderStatusChanged @event)
        {
            switch (@event.Status.IBSStatusId)
            {
                case VehicleStatuses.Common.Loaded:
                {
                    var order = _orderDao.FindById(@event.SourceId);
                    
                    if (_serverSettings.GetPaymentSettings().AutomaticPaymentPairing
                        && _serverSettings.GetPaymentSettings().PaymentMode != PaymentMethod.RideLinqCmt
                        && (order.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id    // Only send notification if using CardOnFile
                            || order.Settings.ChargeTypeId == ChargeTypes.PayPal.Id))   // or PayPal
                    {
                        var account = _accountDao.FindById(@event.Status.AccountId);
                        var response = _paymentFacadeService.Pair(@event.SourceId, account.DefaultTipPercent);
                        _notificationService.SendAutomaticPairingPush(@event.SourceId, account.DefaultTipPercent, response.IsSuccessful);
                    } 
                }
                break;
            }
        }
    }
}
