#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
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
            IEventHandler<UserAddedToPromotionWhiteList>,
            IEventHandler<OrderCancelledBecauseOfIbsError>
    {
        private readonly INotificationService _notificationService;
        private readonly IServerSettings _serverSettings;
        private readonly IPromotionDao _promotionDao;
        private static readonly ILog Log = LogManager.GetLogger(typeof(PushNotificationSender));

        public PushNotificationSender(INotificationService notificationService, IServerSettings serverSettings, IPromotionDao promotionDao)
        {
            _notificationService = notificationService;
            _serverSettings = serverSettings;
            _promotionDao = promotionDao;
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
                    case VehicleStatuses.Common.Loaded:
                        _notificationService.SendPairingInquiryPush(@event.Status);
                        break;
                    case VehicleStatuses.Common.Timeout:
                        if (!_serverSettings.ServerData.Network.Enabled)
                        {
                            _notificationService.SendTimeoutPush(@event.Status);
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
            try
            {
                if (@event.IsNoShowFee)
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

        public void Handle(UserAddedToPromotionWhiteList @event)
        {
            try
            {
                var promotion = _promotionDao.FindById(@event.SourceId);
                _notificationService.SendPromotionUnlockedPush(@event.AccountId, promotion);
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }
        }
        
        public void Handle(OrderCancelledBecauseOfIbsError @event)
        {
            try
            {
                _notificationService.SendOrderCreationErrorPush(@event.SourceId, @event.ErrorDescription);
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }
        }
    }
}