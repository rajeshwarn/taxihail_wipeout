using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Resources;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public interface IPaymentServiceClient
    {
        Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, string nameOnCard, DateTime expiryDate, string cvv, string kountSessionId, string zipCode, Account account);

        Task<BasePaymentResponse> ValidateTokenizedCard(CreditCardDetails creditCard, string cvv, string kountSessionId, Account account);

        Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken);

        Task<OverduePayment> GetOverduePayment();

        Task<SettleOverduePaymentResponse> SettleOverduePayment();

		Task<GenerateClientTokenResponse> GenerateClientTokenResponse();

		Task<TokenizedCreditCardResponse> AddPaymentMethod(string nonce, PaymentMethods method, string cardholderName = null);
    }
}