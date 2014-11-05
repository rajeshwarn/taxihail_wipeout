using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public interface IPaymentServiceClient
    {
        Task ResendConfirmationToDriver(Guid orderId);
        Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, DateTime expiryDate, string cvv);
        Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken);        

        Task<CommitPreauthorizedPaymentResponse> CommitPayment(string cardToken, double amount, double meterAmount,
            double tipAmount, Guid orderId);

        Task<PairingResponse> Pair(Guid orderId, string cardToken, int? autoTipPercentage, double? autoTipAmount);
        Task<BasePaymentResponse> Unpair(Guid orderId);
    }
}