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
        private readonly string _baseUrl;
        private readonly string _sessionId;

        private IPaymentServiceClient _client;

		private static ClientPaymentSettings _cachedSettings;
        
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

		public async Task<ClientPaymentSettings> GetPaymentSettings(bool cleanCache = false)
		{
			if (_cachedSettings == null || cleanCache)
			{
                _cachedSettings = await _serviceClient.GetPaymentSettings().ConfigureAwait(false);
                _client = GetClient(_cachedSettings);
			}
			return _cachedSettings;
		}

		public void ClearPaymentSettingsFromCache()
		{
			// this forces the payment settings to be refreshed at the next call
			// since we can't them at the same time as the standard settings because we could be not authenticated
			_cachedSettings = null;
		    _client = null;
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
        
        public async Task ResendConfirmationToDriver(Guid orderId)
        {
            await _client.ResendConfirmationToDriver(orderId);
        }

        public async Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, DateTime expiryDate, string cvv)
        {
            return await _client.Tokenize(creditCardNumber, expiryDate, cvv);
        }

        public async Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken)
        {
            return await _client.ForgetTokenizedCard(cardToken);
        }

        public async Task<CommitPreauthorizedPaymentResponse> CommitPayment(string cardToken, double amount, double meterAmount, double tipAmount, Guid orderId)
        {
            return await _client.CommitPayment(cardToken, amount, meterAmount, tipAmount, orderId);
        }

        public async Task<PairingResponse> Pair(Guid orderId, string cardToken, int? autoTipPercentage, double? autoTipAmount)
        {
			return await _client.Pair(orderId, cardToken, autoTipPercentage, autoTipAmount);
        }

        public async Task<BasePaymentResponse> Unpair(Guid orderId)
        {
            return await _client.Unpair(orderId);
        }

        private IPaymentServiceClient GetClient(ClientPaymentSettings settings)
        {
            const string onErrorMessage = "Payment Method not found or unknown";

            switch (settings.PaymentMode)
            {
                case PaymentMethod.Braintree:
                    return new BraintreeServiceClient(_baseUrl, _sessionId, settings.BraintreeClientSettings.ClientKey, _packageInfo);

                case PaymentMethod.RideLinqCmt:
                case PaymentMethod.Cmt:
                    return new CmtPaymentClient(_baseUrl, _sessionId, settings.CmtPaymentSettings, _packageInfo, null);

                case PaymentMethod.Moneris:
                    return new MonerisServiceClient(_baseUrl, _sessionId, settings.MonerisPaymentSettings, _packageInfo, _logger);

                case PaymentMethod.Fake:
                    return new FakePaymentClient();

                default:
                    throw new Exception(onErrorMessage);
            }
        }
    }
}