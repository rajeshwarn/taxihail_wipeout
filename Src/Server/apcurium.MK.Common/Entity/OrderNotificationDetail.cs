using System;

namespace apcurium.MK.Common.Entity
{
    public class OrderNotificationDetail
    {
        public Guid Id { get; set; }

        public bool IsTaxiNearbyNotificationSent { get; set; }

        public bool IsUnpairingReminderNotificationSent { get; set; }

        public bool InfoAboutPaymentWasSentToDriver { get; set; }
    }
}