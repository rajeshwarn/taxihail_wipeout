using System;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace apcurium.MK.Booking.Api.Contract.Requests.Payment
{
    [RouteDescription("/order/pairing", "POST")]
    public class CmtPaymentPairingRequest : IReturn<CmtPaymentPairingResponse>
    {
        public string PairingToken { get; set; }
        public Guid OrderUuid { get; set; }

        public long? TimeoutSeconds { get; set; }
    }
}
