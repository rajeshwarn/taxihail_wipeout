using System;
using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.Braintree;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Client.Payments.Fake;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Api.Client.Payments.Moneris;
using apcurium.MK.Common.Diagnostic;


#if IOS
using ServiceStack.ServiceClient.Web;
using ServiceStack.Common.ServiceClient.Web;
#endif
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class PaymentService : BaseService, IPaymentService
    {
		private readonly ConfigurationClientService _serviceClient;
		private readonly ICacheService _cache;
		private readonly IPackageInfo _packageInfo;
		private readonly ILogger _logger;
		private static ClientPaymentSettings _cachedSettings;

        string _baseUrl;
        string _sessionId;
        private const string PayedCacheSuffix = "_Payed";

		public PaymentService(string url, string sessionId, ConfigurationClientService serviceClient, ICacheService cache, IPackageInfo packageInfo, ILogger logger)
        {
			_logger = logger;
			_packageInfo = packageInfo;
            _baseUrl = url;
            _sessionId = sessionId;
            _cache = cache;
			_serviceClient = serviceClient;
        }

		public ClientPaymentSettings GetPaymentSettings(bool cleanCache = false)
		{
			if (_cachedSettings == null || cleanCache)
			{
				_cachedSettings = _serviceClient.GetPaymentSettings();
			}
			return _cachedSettings;
		}

		public void ClearPaymentSettingsFromCache()
		{
			// this forces the payment settings to be refreshed at the next call
			// since we can't them at the same time as the standard settings because we could be not authenticated
			_cachedSettings = null;
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

            var settings = GetPaymentSettings();
            switch (settings.PaymentMode)
            {
                case PaymentMethod.Braintree:
					return new BraintreeServiceClient(_baseUrl, _sessionId, settings.BraintreeClientSettings.ClientKey, _packageInfo.UserAgent);

                case PaymentMethod.RideLinqCmt:
                case PaymentMethod.Cmt:
					return new CmtPaymentClient(_baseUrl, _sessionId, settings.CmtPaymentSettings, _packageInfo.UserAgent, null);

				case PaymentMethod.Moneris:
					return new MonerisServiceClient (_baseUrl, _sessionId, settings.MonerisClientSettings, _packageInfo.UserAgent, _logger);

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