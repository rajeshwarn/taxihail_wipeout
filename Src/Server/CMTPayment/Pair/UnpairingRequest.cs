using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace CMTPayment.Pair
{
    [RouteDescription("init/pairing/{PairingToken}")]
    public class UnpairingRequest : IReturn<CmtUnpairingResponse>
    {
        public string PairingToken { get; set; }
    }
}