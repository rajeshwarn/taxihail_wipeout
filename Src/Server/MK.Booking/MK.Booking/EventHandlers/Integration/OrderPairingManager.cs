using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

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
        private readonly ILogger _logger;
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IPaymentService _paymentFacadeService;
        private readonly IServerSettings _serverSettings;
        private readonly Resources.Resources _resources;

        public OrderPairingManager(INotificationService notificationService, 
            IOrderDao orderDao,
            ICreditCardDao creditCardDao,
            IAccountDao accountDao,
            IPaymentService paymentFacadeService,
            IServerSettings serverSettings,
            ILogger logger,
            Func<BookingDbContext> contextFactory)
        {
            _notificationService = notificationService;
            _orderDao = orderDao;
            _creditCardDao = creditCardDao;
            _accountDao = accountDao;
            _paymentFacadeService = paymentFacadeService;
            _serverSettings = serverSettings;
            _logger = logger;
            _contextFactory = contextFactory;
            _resources = new Resources.Resources(serverSettings);
        }

        public void Handle(OrderStatusChanged @event)
        {

            _logger.LogMessage("OrderPairingManager Handle : " + @event.Status.IBSStatusId);
            
            switch (@event.Status.IBSStatusId)
            {            
                case VehicleStatuses.Common.Loaded:
                {
                    var orderStatus = _orderDao.FindOrderStatusById(@event.SourceId);

                    _logger.LogMessage("OrderPairingManager RideLinqPairingCode : " + orderStatus.RideLinqPairingCode ?? "No code");

                    if (orderStatus.IsPrepaid)
                    {
                        // No need to pair, order was already paid
                        return;
                    }

                    var order = _orderDao.FindById(@event.SourceId);

                    if (order.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id
                        || order.Settings.ChargeTypeId == ChargeTypes.PayPal.Id)
                    {
                        var paymentSettings = _serverSettings.GetPaymentSettings(order.Market);

                        if (paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt
                            && paymentSettings.CmtPaymentSettings.UsePairingCode
                            && !orderStatus.RideLinqPairingCode.HasValue())
                        {
                            return;
                        }

                        var account = _accountDao.FindById(@event.Status.AccountId);
                        var creditCard = _creditCardDao.FindByAccountId(account.Id).FirstOrDefault();
                        var cardToken = creditCard != null ? creditCard.Token : null;
                        var defaultTipPercentage = account.DefaultTipPercent ?? _serverSettings.ServerData.DefaultTipPercentage;

                        var response = _paymentFacadeService.Pair(order.CompanyKey, @event.SourceId, cardToken, defaultTipPercentage);

                        if (!response.IsSuccessful)
                        {
                            UpdateIBSStatusDescription(order.Id, account.Language, "OrderStatus_PairingFailed");
                        }
                        else
                        {
                            UpdateIBSStatusDescription(order.Id, account.Language, "OrderStatus_PairingSuccess");
                         }

                        _notificationService.SendAutomaticPairingPush(@event.SourceId, creditCard, defaultTipPercentage, response.IsSuccessful);
                    } 
                }
                break;
            }
        }

        private void UpdateIBSStatusDescription(Guid orderId, string language, string resourceName)
        {
            using (var context = _contextFactory.Invoke())
            {
                var details = context.Find<OrderStatusDetail>(orderId);
                if (details != null)
                {
                    details.IBSStatusDescription = _resources.Get(resourceName, language ?? SupportedLanguages.en.ToString());
                    context.Save(details);
                }
            }
        }
    }
}
