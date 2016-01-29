using ServiceStack.ServiceHost;

namespace CMTPayment.Tokenize
{
    [Route("tokenize")]
    public class TokenizeRequest : BasePaymentValidationRequest, IReturn<TokenizeResponse>
    {
        public TokenizeRequest()
        {
            ValidateAccountInformation = true;
        }

        /// <summary>
        /// Card number
        /// </summary>
        /// <value>The card number.</value>
        public string AccountNumber { get; set; }

        public string ExpiryDate { get; set; }

        public string Cvv { get; set; }

        /// <summary>
        /// This must be false when testing because we try to tokenize a fake card
        /// </summary>
        /// <value><c>true</c> if validate account information; otherwise, <c>false</c>.</value>
        public bool ValidateAccountInformation { get; set; }

        /// <summary>
        /// Customer's account id (ex: accountId.ToString())
        /// </summary>
        /// <value>The customer identifier.</value>
        public string CustomerId { get; set; }
    }
}