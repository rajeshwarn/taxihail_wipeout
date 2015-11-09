using System;

namespace apcurium.MK.Booking.Api.Contract.Resources.Payments
{
    public class CmtPaymentPairingResponse
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CardOnFileId { get; set; }
        public string TripRequestNumber { get; set; }
    }
}
