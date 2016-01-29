using ServiceStack.ServiceHost;

namespace CMTPayment.Tokenize
{
    [Route("tokenize/validate")]
    public class TokenizeValidateRequest : BasePaymentValidationRequest, IReturn<PaymentResponse>
    {
        public string Token { get; set; }

        public string Cvv { get; set; }
    }
}