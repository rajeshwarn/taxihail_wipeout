using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [Route("/settings/notifications", "GET, POST")]
    [Route("/settings/notifications/{AccountId}", "GET, POST")]
    public class NotificationSettingsRequest
    {
        public Guid? AccountId { get; set; }
        public bool Enabled { get; set; }
        public bool? BookingConfirmationEmail { get; set; }
        public bool? ReceiptEmail { get; set; }
        public bool? DriverAssignedEmail { get; set; }
        public bool? DriverAssignedPush { get; set; }
        public bool? ConfirmPairingPush { get; set; }
        public bool? NearbyTaxiPush { get; set; }
        public bool? VehicleAtPickupPush { get; set; }
        public bool? PaymentConfirmationPush { get; set; }
    }
}