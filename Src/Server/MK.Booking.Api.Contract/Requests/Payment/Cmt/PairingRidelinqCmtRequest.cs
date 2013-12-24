#region

using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment.Cmt
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