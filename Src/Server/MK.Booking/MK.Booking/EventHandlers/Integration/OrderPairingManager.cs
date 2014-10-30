using System;
using System.Linq;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderPairingManager:
        IIntegrationEventHandler,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<OrderSwitchedToNextDispatchCompany>
    {
        private readonly IPairingService _pairingService;
        private readonly INotificationService _notificationService;
        private readonly IServerSettings _serverSettings;
        private readonly IOrderDao _orderDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IAccountDao _accountDao;

        public OrderPairingManager(IPairingService pairingService, 
            INotificationService notificationService, 
            IServerSettings serverSettings,
            IOrderDao orderDao,
            ICreditCardDao creditCardDao,
            IAccountDao accountDao)
        {
            _pairingService = pairingService;
            _notificationService = notificationService;
            _serverSettings = serverSettings;
            _orderDao = orderDao;
            _creditCardDao = creditCardDao;
            _accountDao = accountDao;
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
                        && order.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
                        && creditCardAssociatedToAccount != null)        // Only send notification if using card on file
                    {
                        
                        var account = _accountDao.FindById(@event.Status.AccountId);

                        bool success;
                        try
                        {
                            _pairingService.Pair(@event.SourceId, creditCardAssociatedToAccount.Token, account.DefaultTipPercent);
                            success = true;
                        }
                        catch
                        {
                            success = false;
                        }

                        _notificationService.SendAutomaticPairingPush(@event.SourceId, account.DefaultTipPercent, creditCardAssociatedToAccount.Last4Digits, success);
                    } 
                }
                break;
            }
        }

        public void Handle(OrderSwitchedToNextDispatchCompany @event)
        {
            if (_pairingService != null
                && _serverSettings.GetPaymentSettings().AutomaticPayment
                && _serverSettings.GetPaymentSettings().AutomaticPaymentPairing)
            {
                _pairingService.Unpair(@event.SourceId);
            }
        }
    }
}
