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
        public Guid OrderId { get; set; }
        public string CardToken { get; set; }
        public int? AutoTipPercentage { get; set; }
        public double? AutoTipAmount { get; set; }
    }
}