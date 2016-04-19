using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;
using CMTPayment.Tokenize;

namespace CMTPayment.Authorization
{
    [RouteDescription("payment/{PairingToken}/authorize/{CofToken}")]
    public class CmtRideLinqAuthorizationRequest : BasePaymentValidationRequest, IReturn<UnsuccessfulResponse>
    {
        public string PairingToken { get; set; }

        public string CofToken { get; set; }

        public string LastFour { get; set; }
    }
}