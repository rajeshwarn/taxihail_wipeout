using ServiceStack.ServiceHost;

namespace CMTPayment.Actions
{
    [Route("payment/{PairingToken}/credit")]
    public class CmtRideLinqRefundRequest : IReturn<PaymentResponse>
    {
        public string CofToken { get; set; }

        public string LastFour { get; set; }

        public int AuthAmount { get; set; }
    }
}
