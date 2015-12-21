#region

using System;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;
using log4net;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class PushNotificationSender : IIntegrationEventHandler,
            IEventHandler<OrderStatusChanged>,
            IEventHandler<CreditCardPaymentCaptured_V2>,
            IEventHandler<OrderPreparedForNextDispatch>,
            IEventHandler<UserAddedToPromotionWhiteList_V2>,
            IEventHandler<OrderCancelledBecauseOfError>,
            IEventHandler<CreditCardDeactivated>
    {
        private readonly IAccountDao _accountDao;
        private readonly INotificationService _notificationService;
        private readonly IServerSettings _serverSettings;
        private readonly IPromotionDao _promotionDao;
        private static readonly ILog Log = LogManager.GetLogger(typeof(PushNotificationSender));

        public PushNotificationSender(INotificationService notificationService, IServerSettings serverSettings, IPromotionDao promotionDao, IAccountDao accountDao)
        {
            _notificationService = notificationService;
            _serverSettings = serverSettings;
            _promotionDao = promotionDao;
            _accountDao = accountDao;
        }

        public void Handle(OrderStatusChanged @event)
        {
            try
            {
                switch (@event.Status.IBSStatusId)
                {
                    case VehicleStatuses.Common.Assigned:
                        _notificationService.SendAssignedPush(@event.Status);
                        break;
                    case VehicleStatuses.Common.Arrived:
                        _notificationService.SendArrivedPush(@event.Status);
                        break;
                    case VehicleStatuses.Common.Timeout:
                        if (!_serverSettings.ServerData.Network.Enabled)
                        {
                            _notificationService.SendTimeoutPush(@event.Status);
                        }
                        break;
                    case VehicleStatuses.Common.Waiting:
                        if (@event.PreviousIBSStatusId == VehicleStatuses.Common.Assigned)
                        {
                            _notificationService.SendBailedPush(@event.Status);
                        }
                        break;
                    default:
                        // No push notification for this order status
                        return;
                }
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            @event.MigrateFees();

            try
            {
                if (@event.IsForPrepaidOrder || @event.FeeType != FeeTypes.None)
                {
                    // Don't message user for now
                    return;
                }

                _notificationService.SendPaymentCapturePush(@event.OrderId, @event.Amount);
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }
        }

        public void Handle(OrderPreparedForNextDispatch @event)
        {
            try
            {
                _notificationService.SendChangeDispatchCompanyPush(@event.SourceId);
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }
        }

        public void Handle(UserAddedToPromotionWhiteList_V2 @event)
        {
            try
            {
                var promotion = _promotionDao.FindById(@event.SourceId);

                foreach (var accountId in @event.AccountIds)
                {
                    _notificationService.SendPromotionUnlockedPush(accountId, promotion);
                }
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }
        }
        
        public void Handle(OrderCancelledBecauseOfError @event)
        {
            if (@event.CancelWasRequested)
            {
                Log.Info(string.Format("Received OrderCancelledBecauseOfError event but cancel was requested by user before, no need to inform him of order creation failure (OrderId: {0}).", @event.SourceId));
                return;
            }

            try
            {
                _notificationService.SendOrderCreationErrorPush(@event.SourceId, @event.ErrorDescription);
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }
        }

        public void Handle(CreditCardDeactivated @event)
        {
            try
            {
                var account = _accountDao.FindById(@event.SourceId);
                _notificationService.SendCreditCardDeactivatedPush(account);
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }            
        }
    }
}