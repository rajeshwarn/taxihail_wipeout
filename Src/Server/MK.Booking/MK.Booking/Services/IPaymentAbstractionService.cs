using System;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Services
{
    public interface IPaymentFacadeService
    {
        bool IsPayPal(Guid? accountId = null, Guid? orderId = null);
        PaymentProvider ProviderType(Guid orderId);
        PreAuthorizePaymentResponse PreAuthorize(Guid orderId, AccountDetail account, decimal amountToPreAuthorize);
        CommitPreauthorizedPaymentResponse CommitPayment(Guid orderId, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId);
        PairingResponse Pair(Guid orderId, int? autoTipPercentage);
        BasePaymentResponse Unpair(Guid orderId);
        void VoidPreAuthorization(Guid orderId);
        void VoidTransaction(Guid orderId, string transactionId, ref string message);
    }
}