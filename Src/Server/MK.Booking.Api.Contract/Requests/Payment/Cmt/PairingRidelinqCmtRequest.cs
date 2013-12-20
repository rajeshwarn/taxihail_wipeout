using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Contract.Requests.Cmt
{
    [Authenticate]
    [Route("/payments/cmt/pair", "POST")]
    public class PairingRidelinqCmtRequest : IReturn<PairingResponse>
    {
        public string Medallion { get; set; }
        public string DriverId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool AutoCompletePayment { get; set; }
        public int? AutoTipPercentage { get; set; }
        public double? AutoTipAmount { get; set; }
    }
}