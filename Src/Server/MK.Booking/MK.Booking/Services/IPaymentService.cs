using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Services
{
    public interface IPaymentService
    {
        bool IsPayPal(Guid? accountId = null, Guid? orderId = null, bool isForPrepaid = false);

        PaymentProvider ProviderType(string companyKey, Guid? orderId = null);

        PreAuthorizePaymentResponse PreAuthorize(string companyKey, Guid orderId, AccountDetail account, decimal amountToPreAuthorize, bool isReAuth = false, bool isSettlingOverduePayment = false, bool isForPrepaid = false, string cvv = null);

        CommitPreauthorizedPaymentResponse CommitPayment(string companyKey, Guid orderId, AccountDetail account, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId, string reAuthOrderId = null, bool isForPrepaid = false);

        Task<RefundPaymentResponse> RefundPayment(string companyKey, Guid orderId);

        Task<BasePaymentResponse> UpdateAutoTip(string companyKey, Guid orderId, int autoTipPercentage);
        
        DeleteTokenizedCreditcardResponse DeleteTokenizedCreditcard(string cardToken);

        Task<PairingResponse> Pair(string companyKey, Guid orderId, string cardToken, int autoTipPercentage);

        Task<BasePaymentResponse> Unpair(string companyKey, Guid orderId);

        void VoidPreAuthorization(string companyKey, Guid orderId, bool isForPrepaid = false);

        void VoidTransaction(string companyKey, Guid orderId, string transactionId, ref string message);
    }
}