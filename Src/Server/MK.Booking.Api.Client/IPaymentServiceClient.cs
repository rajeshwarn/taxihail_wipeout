using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Resources;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public interface IPaymentServiceClient
    {
        Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, DateTime expiryDate, string cvv, string kountSessionId, string zipCode, Account account);

        Task<BasePaymentResponse> ValidateTokenizedCard(string cardToken, string cvv, string kountSessionId, string zipCode, Account account);

        Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken);

        Task<OverduePayment> GetOverduePayment();

        Task<SettleOverduePaymentResponse> SettleOverduePayment();
    }
}