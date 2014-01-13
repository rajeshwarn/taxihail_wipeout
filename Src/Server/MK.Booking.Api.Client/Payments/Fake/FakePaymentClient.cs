#region

using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;

#endregion

namespace apcurium.MK.Booking.Api.Client.Payments.Fake
{
    public class FakePaymentClient : IPaymentServiceClient
    {
        private readonly Random _random;

        public FakePaymentClient()
        {
            _random = new Random();
        }

        public Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, DateTime expiryDate, string cvv)
        {
            return Task.FromResult(new TokenizedCreditCardResponse
            {
                CardOnFileToken = "4043702891740165y",
                CardType = "Visa",
                LastFour = creditCardNumber.Substring(creditCardNumber.Length - 4),
                IsSuccessfull = true,
                Message = "Success"
            });
        }

        public Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken)
        {
            return Task.FromResult(new DeleteTokenizedCreditcardResponse
            {
                IsSuccessfull = true,
                Message = "Success"
            });
        }

        public Task<PreAuthorizePaymentResponse> PreAuthorize(string cardToken, double amount, double meterAmount,
            double tipAmount, Guid orderId)
        {
            return Task.FromResult(new PreAuthorizePaymentResponse
            {
                TransactionId = 100000000 + _random.Next(999) + "",
                IsSuccessfull = true
            });
        }

        public Task<CommitPreauthorizedPaymentResponse> CommitPreAuthorized(string transactionId)
        {
            return Task.FromResult(new CommitPreauthorizedPaymentResponse
            {
                IsSuccessfull = true,
                Message = "Success"
            });
        }

        public Task<CommitPreauthorizedPaymentResponse> PreAuthorizeAndCommit(string cardToken, double amount,
            double meterAmount, double tipAmount, Guid orderId)
        {
            return Task.FromResult(new CommitPreauthorizedPaymentResponse
            {
                IsSuccessfull = true,
                Message = "Success"
            });
        }

        public Task<PairingResponse> Pair(Guid orderId, string cardToken, int? autoTipPercentage, double? autoTipAmount)
        {
            throw new NotImplementedException();
        }

        public Task<BasePaymentResponse> Unpair(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public Task ResendConfirmationToDriver(Guid orderId)
        {
            return Task.FromResult(true);
        }
    }
}