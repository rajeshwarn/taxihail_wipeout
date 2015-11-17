namespace CMTPayment.Tokenize
{
    [Route("tokenize")]
    public class TokenizeRequest : IReturn<TokenizeResponse>
    {
        public TokenizeRequest()
        {
            ValidateAccountInformation = true;
        }

        public string AccountNumber { get; set; }

        public string ExpiryDate { get; set; }

        public bool ValidateAccountInformation { get; set; }

        public string Cvv { get; set; }

        public string ZipCode { get; set; }
    }
}