using ServiceStack.ServiceHost;

namespace CMTPayment.Authorization
{
    [Route("payment/{PairingToken}/authorize/{CofToken}")]
    public class CmtRideLinqAuthorizationRequest : IReturn<UnsuccessfulResponse>
    {
        public string PairingToken { get; set; }

        public string CofToken { get; set; }
    }
}