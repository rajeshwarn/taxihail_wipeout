using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize
{
    /// <summary>
    /// The Tokenize resource provides developers the ability to create a token in place of 
    /// cardholder data, update tokenized data and delete a token. The token does not use the 
    /// cardholder data to create the token so there is no way to get the cardholder information 
    /// with just the token alone
    /// </summary>
    public class CmtPaymentTokenizeClient : CmtPaymentServiceClient
    {
        public TokenizeResponse Tokenize(string accountNumber, DateTime expiryDate)
        {
            return Client.Post<TokenizeResponse>(new TokenizeRequest()
            {
                AccountNumber = accountNumber,
                ExpiryDateYYMM = expiryDate.ToString("yyMM")
            });
        }


        public PaymentResponse ForgetTokenizedCard(string cardToken)
        {
            return Client.Delete<TokenizeDeleteResponse>(new TokenizeDeleteRequest()
                {
                    CardToken = cardToken
                });
        }
    }
}
