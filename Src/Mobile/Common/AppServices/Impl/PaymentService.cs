using System;
using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Client.Payments.Fake;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Booking.Api.Client.Payments.Braintree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
#if IOS
using ServiceStack.ServiceClient.Web;
using ServiceStack.Common.ServiceClient.Web;
#endif

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class PaymentService : BaseService, IPaymentService
    {
        IConfigurationManager _configurationManager;
        string _baseUrl;
        string _sessionId;
        ICacheService _cache;
        private const string PayedCacheSuffix = "_Payed";

        public PaymentService(string url, string sessionId, IConfigurationManager configurationManager, ICacheService cache)
        {
            _baseUrl = url;
            _sessionId = sessionId;
            _cache = cache;
            _configurationManager = configurationManager;
        }

        public double? GetPaymentFromCache(Guid orderId)
        {
            var result = _cache.Get<string>(orderId + PayedCacheSuffix);
            double amount;

            if (double.TryParse(result, out amount))
            {
                return amount;
            }
            return null;
        }

        public void SetPaymentFromCache(Guid orderId, double amount)
        {
            _cache.Set(orderId + PayedCacheSuffix, amount.ToString(CultureInfo.InvariantCulture));
        }

        public IPaymentServiceClient GetClient()
        {
            const string onErrorMessage = "Payment Method not found or unknown";

            var settings = _configurationManager.GetPaymentSettings();
            switch (settings.PaymentMode)
            {
                case PaymentMethod.Braintree:
                    return new BraintreeServiceClient(_baseUrl, _sessionId, settings.BraintreeClientSettings.ClientKey, TinyIoCContainer.Current.Resolve<IPackageInfo>().UserAgent);

                case PaymentMethod.RideLinqCmt:
                case PaymentMethod.Cmt:
                    return new CmtPaymentClient(_baseUrl, _sessionId, settings.CmtPaymentSettings, TinyIoCContainer.Current.Resolve<IPackageInfo>().UserAgent);

                case PaymentMethod.Fake:
                    return new FakePaymentClient();
                default:
                    throw new Exception(onErrorMessage);
            }
        }

        public Task ResendConfirmationToDriver(Guid orderId)
        {
            return GetClient().ResendConfirmationToDriver(orderId);
        }

        public Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, DateTime expiryDate, string cvv)
        {
            return GetClient().Tokenize(creditCardNumber, expiryDate, cvv);
        }

        public Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken)
        {
            return GetClient().ForgetTokenizedCard(cardToken);
        }

        public Task<PreAuthorizePaymentResponse> PreAuthorize(string cardToken, double amount, double meterAmount, double tipAmount, Guid orderId)
        {
            return GetClient().PreAuthorize(cardToken, amount, meterAmount, tipAmount, orderId);
        }

        public Task<CommitPreauthorizedPaymentResponse> CommitPreAuthorized(string transactionId)
        {
            return GetClient().CommitPreAuthorized(transactionId);
        }

        public Task<CommitPreauthorizedPaymentResponse> PreAuthorizeAndCommit(string cardToken, double amount, double meterAmount, double tipAmount, Guid orderId)
        {
            return GetClient().PreAuthorizeAndCommit(cardToken, amount, meterAmount, tipAmount, orderId);
        }

        public Task<PairingResponse> Pair(Guid orderId, string cardToken, int? autoTipPercentage, double? autoTipAmount)
        {
            return GetClient().Pair(orderId, cardToken, autoTipPercentage, autoTipAmount);
        }

        public Task<BasePaymentResponse> Unpair(Guid orderId)
        {
            return GetClient().Unpair(orderId);
        }
    }
}