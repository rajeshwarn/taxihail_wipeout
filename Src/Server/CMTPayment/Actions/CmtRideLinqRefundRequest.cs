using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace CMTPayment.Actions
{
    [RouteDescription("payment/{PairingToken}/credit")]
    public class CmtRideLinqRefundRequest : IReturn<UnsuccessfulResponse>
    {
        public string PairingToken { get; set; }

        public string CofToken { get; set; }

        public string LastFour { get; set; }

        public int AuthAmount { get; set; }
    }
}
