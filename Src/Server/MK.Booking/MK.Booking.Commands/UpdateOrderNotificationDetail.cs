using Infrastructure.Messaging;
using System;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateOrderNotificationDetail : ICommand
    {
        public UpdateOrderNotificationDetail()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public bool? IsTaxiNearbyNotificationSent { get; set; }

        public bool? IsUnpairingReminderNotificationSent { get; set; }

        public bool? InfoAboutPaymentWasSentToDriver { get; set; }
    }
}