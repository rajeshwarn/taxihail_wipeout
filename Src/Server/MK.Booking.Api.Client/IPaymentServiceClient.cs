using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public interface IPaymentServiceClient
    {
        Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, DateTime expiryDate, string cvv, string zipCode = null);

        Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken);

        Task<OverduePayment> GetOverduePayment();

        Task<SettleOverduePaymentResponse> SettleOverduePayment();

		Task<GenerateClientTokenResponse> GenerateClientTokenResponse();

		Task<TokenizedCreditCardResponse> AddPaymentMethod(string nonce);
    }
}