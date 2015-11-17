namespace CMTPayment.Tokenize
{
    [Route("tokenize/{CardToken}/")]
    public class TokenizeDeleteRequest : IReturn<TokenizeDeleteResponse>
    {
        public string CardToken { get; set; }
    }
}