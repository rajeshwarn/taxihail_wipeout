using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class OrderNotificationDetailUpdated:VersionedEvent
    {
        public bool? IsTaxiNearbyNotificationSent { get; set; }

        public bool? IsUnpairingReminderNotificationSent { get; set; }

        public bool? InfoAboutPaymentWasSentToDriver { get; set; }
    }
}