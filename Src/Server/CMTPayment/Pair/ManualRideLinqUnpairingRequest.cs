using ServiceStack.ServiceHost;

namespace CMTPayment.Pair
{
    [Route("init/pairing/{PairingToken}")]
    public class ManualRideLinqUnpairingRequest : IReturn<CmtUnpairingResponse>
    {
        public string PairingToken { get; set; }
    }
}
