using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MK.Common.Configuration
{
    public class NotificationSettings
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayName("AllowNotifications")]
        public bool Enabled { get; set; }

        public bool? BookingConfirmationEmail { get; set; }
        public bool? ReceiptEmail { get; set; }

        public bool? DriverAssignedPush { get; set; }
        public bool? ConfirmPairingPush { get; set; }
        public bool? NearbyTaxiPush { get; set; }
        public bool? VehicleAtPickupPush { get; set; }
        public bool? PaymentConfirmationPush { get; set; }
    }
}