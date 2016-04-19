using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

namespace CMTPayment.Tokenize
{
    [RouteDescription("tokenize/{CardToken}/")]
    public class TokenizeDeleteRequest : IReturn<TokenizeDeleteResponse>
    {
        public string CardToken { get; set; }
    }
}