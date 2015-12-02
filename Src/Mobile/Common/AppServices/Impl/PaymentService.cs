using System;
using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments;
using apcurium.MK.Booking.Api.Client.Payments.Braintree;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Client.Payments.Fake;
using apcurium.MK.Booking.Api.Client.Payments.Moneris;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Resources;
#if IOS
using ServiceStack.ServiceClient.Web;
using ServiceStack.Common.ServiceClient.Web;
#endif

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

		private static IPaymentServiceClient _client;
		private static ClientPaymentSettings _cachedSettings;
        
        private const string PayedCacheSuffix = "_Payed";
		private const string PaymentSettingsCacheKey = "PaymentSettings";
		private const string OnErrorMessage = "Payment Method not found or unknown";

		public PaymentService(string url, string sessionId,
            ConfigurationClientService serviceClient,
            ICacheService cache,
            IPackageInfo packageInfo,
            ILogger logger)
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

			_cachedSettings = _cache.Get<ClientPaymentSettings>(PaymentSettingsCacheKey);

			if (_cachedSettings == null || cleanCache)
			{
			    await RefreshPaymentSettings();

			    _client = GetClient(_cachedSettings);

			    return _cachedSettings;
			}
		    // set client with cached settings for now
		    _client = GetClient (_cachedSettings);

		    // Update cache...
		    Task.Run(() => RefreshPaymentSettings()).FireAndForget();

		    // ... and return current settings
		    return _cachedSettings;
		}

		private async Task RefreshPaymentSettings()
		{
		    try
		    {
                _cachedSettings = await _serviceClient.GetPaymentSettings().ConfigureAwait(false);
                _cache.Set(PaymentSettingsCacheKey, _cachedSettings);
                _client = GetClient(_cachedSettings);
            }
		    catch (Exception ex)
		    {
		        
		        throw;
		    }
			
		}

		public void ClearPaymentSettingsFromCache()
		{
			// this forces the payment settings to be refreshed at the next call
			// since we can't them at the same time as the standard settings because we could be not authenticated
			_cachedSettings = null;
		    _client = null;
			_cache.Clear(PaymentSettingsCacheKey);
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

		public Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, DateTime expiryDate, string cvv, string zipCode = null)
        {
			return GetClient().Tokenize(creditCardNumber, expiryDate, cvv, zipCode);
        }

        public async Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken)
        {
			return await GetClient().ForgetTokenizedCard(cardToken);
        }

		public async Task<OverduePayment> GetOverduePayment()
		{
			return await GetClient().GetOverduePayment();
		}

        public async Task<SettleOverduePaymentResponse> SettleOverduePayment()
        {
            return await GetClient().SettleOverduePayment();
        }

        public async Task<BasePaymentResponse> Unpair(Guid orderId)
        {
            return await new PairingServiceClient(_baseUrl, _sessionId, _packageInfo)
                .Unpair(orderId);
        }

        public async Task<bool> UpdateAutoTip(Guid orderId, int autoTipPercentage)
        {
            return await new PairingServiceClient(_baseUrl, _sessionId, _packageInfo)
                .UpdateAutoTip(orderId, autoTipPercentage);
        }

		private IPaymentServiceClient GetClient()
		{
			if(_client == null)
			{
				throw new Exception(OnErrorMessage);
			}

			return _client;
		}

        private IPaymentServiceClient GetClient(ClientPaymentSettings settings)
        {
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
					return null;
            }
        }
    }
}