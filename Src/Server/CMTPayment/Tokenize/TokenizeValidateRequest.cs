using ServiceStack.ServiceHost;

namespace CMTPayment.Tokenize
{
    [Route("tokenize/validate")]
    public class TokenizeValidateRequest : IReturn<PaymentResponse>
    {
        public string Token { get; set; }

        public string Cvv { get; set; }

        public string ZipCode { get; set; }

        public string SessionId { get; set; }
    }
}