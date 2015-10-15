using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Resources;


namespace apcurium.MK.Booking.Api.Client.Payments.Fake
{
    public class FakePaymentClient : IPaymentServiceClient
    {
        public Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, DateTime expiryDate, string cvv, string zipCode = null)
        {
            return Task.FromResult(new TokenizedCreditCardResponse
            {
                CardOnFileToken = "4043702891740165y",
                CardType = "Visa",
                LastFour = creditCardNumber.Substring(creditCardNumber.Length - 4),
                IsSuccessful = true,
                Message = "Success"
            });
        }

        public Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken)
        {
            return Task.FromResult(new DeleteTokenizedCreditcardResponse
            {
                IsSuccessful = true,
                Message = "Success"
            });
        }

        public Task<OverduePayment> GetOverduePayment()
        {
            return Task.FromResult((OverduePayment)null);
        }

        public Task<SettleOverduePaymentResponse> SettleOverduePayment()
        {
            return Task.FromResult(new SettleOverduePaymentResponse
            {
                IsSuccessful = true,
                Message = "Success"
            });
        }
    }
}