using System;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Services
{
    public interface IPaymentService
    {
        bool IsPayPal(Guid? accountId = null, Guid? orderId = null, bool isForPrepaid = false);

        PaymentProvider ProviderType(Guid? orderId = null);

        PreAuthorizePaymentResponse PreAuthorize(Guid orderId, AccountDetail account, decimal amountToPreAuthorize, bool isReAuth = false, bool isSettlingOverduePayment = false, bool isForPrepaid = false, string cvv = null);

        CommitPreauthorizedPaymentResponse CommitPayment(Guid orderId, AccountDetail account, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId, string reAuthOrderId = null, bool isForPrepaid = false);

        BasePaymentResponse RefundPayment(Guid orderId);

        BasePaymentResponse UpdateAutoTip(Guid orderId, int autoTipPercentage);
        
        DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(string cardToken);

        PairingResponse Pair(Guid orderId, string cardToken, int autoTipPercentage);

        BasePaymentResponse Unpair(Guid orderId);

        void VoidPreAuthorization(Guid orderId, bool isForPrepaid = false);

        void VoidTransaction(Guid orderId, string transactionId, ref string message);
    }
}