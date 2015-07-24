using Infrastructure.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Events
{
    public class OrderNotificationDetailUpdated:VersionedEvent
    {
        public Guid OrderId { get; set; }

        public bool? IsTaxiNearbyNotificationSent { get; set; }

        public bool? IsUnpairingReminderNotificationSent { get; set; }

        public bool? InfoAboutPaymentWasSentToDriver { get; set; }
    }
}