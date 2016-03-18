using CMTPayment.Tokenize;
using ServiceStack.ServiceHost;

namespace CMTPayment.Authorization
{
    [Route("payment/{PairingToken}/authorize/{CofToken}")]
    public class CmtRideLinqAuthorizationRequest : BasePaymentValidationRequest, IReturn<UnsuccessfulResponse>
    {
        public string PairingToken { get; set; }

        public string CofToken { get; set; }

        public string LastFour { get; set; }
    }
}