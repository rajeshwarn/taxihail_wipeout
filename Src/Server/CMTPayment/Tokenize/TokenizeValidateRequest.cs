using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace CMTPayment.Tokenize
{
    [RouteDescription("tokenize/validate")]
    public class TokenizeValidateRequest : BasePaymentValidationRequest, IReturn<PaymentResponse>
    {
        public string Token { get; set; }

        public string Cvv { get; set; }
    }
}