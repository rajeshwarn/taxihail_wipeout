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
using apcurium.MK.Booking.Resources;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class PushNotificationSender : IIntegrationEventHandler,
            IEventHandler<OrderStatusChanged>,
            IEventHandler<CreditCardPaymentCaptured>
    {
        private readonly INotificationService _notificationService;

        public PushNotificationSender(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void Handle(OrderStatusChanged @event)
        {
            _notificationService.SendStatusChangedNotification(@event.Status);
        }

        public void Handle(CreditCardPaymentCaptured @event)
        {
            _notificationService.SendPaymentCaptureNotification(@event.OrderId, @event.Amount);
        }
    }
}