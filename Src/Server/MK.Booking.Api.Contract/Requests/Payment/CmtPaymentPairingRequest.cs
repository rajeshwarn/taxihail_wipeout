using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [Route("/order/pairing", "POST")]
    public class CmtPaymentPairingRequest : IReturn<CmtPaymentPairingResponse>
    {
        public string PairingToken { get; set; }
        public Guid OrderUuid { get; set; }

        public long? TimeoutSeconds { get; set; }
    }
}
