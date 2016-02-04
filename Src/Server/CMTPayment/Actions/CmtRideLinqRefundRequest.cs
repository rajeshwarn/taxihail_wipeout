using ServiceStack.ServiceHost;

namespace CMTPayment.Actions
{
    [Route("payment/{PairingToken}/credit")]
    public class CmtRideLinqRefundRequest : IReturn<PaymentResponse>
    {
        public string CardOfFileToken { get; set; }

        public string LastFourDigits { get; set; }

        public int AuthAmount { get; set; }
    }
}
