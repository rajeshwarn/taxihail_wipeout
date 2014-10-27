using ServiceStack.ServiceHost;

namespace CMTPayment.Tokenize
{
    [Route("v2/tokenize/{CardToken}/")]
    public class TokenizeDeleteRequest : IReturn<TokenizeDeleteResponse>
    {
        public string CardToken { get; set; }
    }
}