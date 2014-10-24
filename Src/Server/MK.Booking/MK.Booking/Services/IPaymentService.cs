using System;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Services
{
    public interface IPaymentService
    {
        PreAuthorizePaymentResponse PreAuthorize(Guid orderId, string email, string cardToken, decimal amountToPreAuthorize);
        CommitPreauthorizedPaymentResponse CommitPayment(decimal amount, decimal meterAmount, decimal tipAmount, string cardToken, Guid orderId, bool isNoShowFee);
        DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(string cardToken);
        PairingResponse Pair(Guid orderId, string cardToken, int? autoTipPercentage, double? autoTipAmount);
        BasePaymentResponse Unpair(Guid orderId);
        void VoidPreAuthorization(Guid orderId);
    }
}