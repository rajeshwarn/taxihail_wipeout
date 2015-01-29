using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public interface IPaymentServiceClient
    {
        Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, DateTime expiryDate, string cvv);
        Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken);

        Task<PairingResponse> Pair(Guid orderId, string cardToken, int? autoTipPercentage);
        Task<BasePaymentResponse> Unpair(Guid orderId);
    }
}