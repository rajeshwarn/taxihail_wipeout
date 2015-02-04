using System;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Services
{
    public interface IPaymentService
    {
        PaymentProvider ProviderType { get; }
        PreAuthorizePaymentResponse PreAuthorize(Guid orderId, string email, string cardToken, decimal amountToPreAuthorize, bool isReAuth = false);
        CommitPreauthorizedPaymentResponse CommitPayment(Guid orderId, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId);
        DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(string cardToken);
        PairingResponse Pair(Guid orderId, string cardToken, int? autoTipPercentage);
        BasePaymentResponse Unpair(Guid orderId);
        void VoidPreAuthorization(Guid orderId);
        void VoidTransaction(Guid orderId, string transactionId, ref string message);
    }
}