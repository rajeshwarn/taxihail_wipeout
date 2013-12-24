#region

using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Pair
{
    [Route("v1/init/pairing/{PairingToken}")]
    public class UnpairingRequest : IReturn<CmtUnpairingResponse>
    {
        public string PairingToken { get; set; }
    }
}