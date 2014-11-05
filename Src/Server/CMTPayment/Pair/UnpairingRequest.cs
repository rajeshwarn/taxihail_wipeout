using ServiceStack.ServiceHost;

namespace CMTPayment.Pair
{
    [Route("v1/init/pairing/{PairingToken}")]
    public class UnpairingRequest : IReturn<CmtUnpairingResponse>
    {
        public string PairingToken { get; set; }
    }
}