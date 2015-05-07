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
        private readonly IOrderDao _orderDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IAccountDao _accountDao;
        private readonly IPaymentService _paymentFacadeService;
        private readonly IServerSettings _serverSettings;

        public OrderPairingManager(INotificationService notificationService, 
            IOrderDao orderDao,
            ICreditCardDao creditCardDao,
            IAccountDao accountDao,
            IPaymentService paymentFacadeService,
            IServerSettings serverSettings
            )
        {
            _notificationService = notificationService;
            _orderDao = orderDao;
            _creditCardDao = creditCardDao;
            _accountDao = accountDao;
            _paymentFacadeService = paymentFacadeService;
            _serverSettings = serverSettings;
        }

        public void Handle(OrderStatusChanged @event)
        {
            switch (@event.Status.IBSStatusId)
            {
                case VehicleStatuses.Common.Loaded:
                {
                    var orderStatus = _orderDao.FindOrderStatusById(@event.SourceId);
                    if (orderStatus.IsPrepaid)
                    {
                        // No need to pair, order was already paid
                        return;
                    }

                    var order = _orderDao.FindById(@event.SourceId);

                    

                    if (order.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
                        || order.Settings.ChargeTypeId == ChargeTypes.PayPal.Id)
                    {
                        if ( ( _serverSettings.GetPaymentSettings().PaymentMode == PaymentMethod.RideLinqCmt ) &&
                              _serverSettings.ServerData.UsePairingCodeWhenUsingRideLinqCmtPayment &&
                              string.IsNullOrWhiteSpace( orderStatus.RideLinqPairingCode ))
                        {
                            return;
                        }

                        var account = _accountDao.FindById(@event.Status.AccountId);
                        var creditCard = _creditCardDao.FindByAccountId(account.Id).FirstOrDefault();
                        var cardToken = creditCard != null ? creditCard.Token : null;

                        var response = _paymentFacadeService.Pair(@event.SourceId, cardToken, account.DefaultTipPercent.HasValue ? account.DefaultTipPercent.Value : _serverSettings.ServerData.DefaultTipPercentage);
                        
                        _notificationService.SendAutomaticPairingPush(@event.SourceId, account.DefaultTipPercent, response.IsSuccessful);
                    } 
                }
                break;
            }
        }
    }
}
