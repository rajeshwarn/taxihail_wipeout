using Infrastructure.EventSourcing;
using System;

namespace apcurium.MK.Booking.Events
{
    public class OrderNotificationDetailUpdated : VersionedEvent
    {
        public Guid OrderId { get; set; }

        public bool? IsTaxiNearbyNotificationSent { get; set; }

        public bool? IsUnpairingReminderNotificationSent { get; set; }

        public bool? InfoAboutPaymentWasSentToDriver { get; set; }

        public bool? NoShowWarningSent { get; set; }
    }
}