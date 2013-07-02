using System;
using MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments
{
    public class CmtFakeClient : IPaymentServiceClient
    {
        private Random _random;

        public CmtFakeClient()
        {
            _random = new Random();
        }

        public TokenizeResponse Tokenize(string creditCardNumber, DateTime expiryDate, string cvv)
        {
            return new TokenizeResponse()
                {
                    CardOnFileToken = "4043702891740165y",
                    CardType = "Visa",
                    LastFour = creditCardNumber.Substring(creditCardNumber.Length-4),
                    ResponseCode = 0,
                    ResponseMessage = "Success"
                };
        }

        public TokenizeDeleteResponse ForgetTokenizedCard(string cardToken)
        {
            return new TokenizeDeleteResponse()
                {
                    ResponseCode = 0,
                    ResponseMessage = "Success"
                };
        }

        public string PreAuthorize(string cardToken, string encryptedCvv, double amount, string orderNumber)
        {
            return 100000000 + _random.Next(999)+"";
        }

        public bool CommitPreAuthorized(string transactionId, string orderNumber)
        {
            return true;
        }
    }
}