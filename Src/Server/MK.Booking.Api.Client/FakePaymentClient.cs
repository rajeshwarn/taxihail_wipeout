using System;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments.CmtPayments
{
    public class FakePaymentClient : IPaymentServiceClient
    {
        private readonly Random _random;

        public FakePaymentClient()
        {
            _random = new Random();
        }

        public TokenizedCreditCardResponse Tokenize(string creditCardNumber, DateTime expiryDate, string cvv)
        {
            return new TokenizedCreditCardResponse()
                {
                    CardOnFileToken = "4043702891740165y",
                    CardType = "Visa",
                    LastFour = creditCardNumber.Substring(creditCardNumber.Length-4),
                    IsSuccessfull = true,
                    Message = "Success"
                };
        }

        public DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken)
        {
            return new DeleteTokenizedCreditcardResponse()
                {
                    IsSuccessfull = true,
                    Message = "Success"
                };
        }

        public PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, string orderNumber)
        {
            return new PreAuthorizePaymentResponse
                {
                    TransactionId = 100000000 + _random.Next(999) + ""
                };
        }

        public CommitPreauthoriedPaymentResponse CommitPreAuthorized(string transactionId, string orderNumber)
        {
            return new CommitPreauthoriedPaymentResponse()
                {
                    IsSuccessfull = true,
                    Message = "Success"
                };
        }
    }
}