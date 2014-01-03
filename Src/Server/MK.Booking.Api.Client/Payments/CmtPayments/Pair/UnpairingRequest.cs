using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Pair
{
    [Route("v1/init/pairing/{PairingToken}")]
    public class UnpairingRequest : IReturn<CmtUnpairingResponse>
    {
        public string PairingToken { get; set; }
    }
}