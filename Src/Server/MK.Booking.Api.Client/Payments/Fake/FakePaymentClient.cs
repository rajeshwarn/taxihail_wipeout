using System;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

namespace apcurium.MK.Booking.Api.Client
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

        public PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, double meterAmount, double tipAmount, Guid orderId)
        {
            return new PreAuthorizePaymentResponse
                {
                    TransactionId = 100000000 + _random.Next(999) + "",
                    IsSuccessfull = true
                };
        }

        public CommitPreauthorizedPaymentResponse CommitPreAuthorized(string transactionId)
        {
            return new CommitPreauthorizedPaymentResponse()
                {
                    IsSuccessfull = true,
                    Message = "Success"
                };
        }

        public CommitPreauthorizedPaymentResponse PreAuthorizeAndCommit(string cardToken, double amount, double meterAmount, double tipAmount, Guid orderId)
        {
            return new CommitPreauthorizedPaymentResponse()
            {
                IsSuccessfull = true,
                Message = "Success"
            };
        }

        public void ResendConfirmationToDriver(Guid orderId)
        {
            //Client.Post(new ResendPaymentConfirmationRequest { OrderId = orderId });
        }
    }
}