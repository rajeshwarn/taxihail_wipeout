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
using Cirrious.CrossCore;

using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class PaymentService : BaseService, IPaymentService
    {
        private readonly ICacheService _cache;
        private readonly IIPAddressManager _ipAddressManager;
		private readonly IPackageInfo _packageInfo;
		private readonly IConnectivityService _connectivityService;
        private readonly ILogger _logger; 
        private readonly string _baseUrl;
	    private readonly Func<string> _sessionId;

	    private static IPaymentServiceClient _client;
		private static ClientPaymentSettings _cachedSettings;
        
        private const string PayedCacheSuffix = "_Payed";
		private const string PaymentSettingsCacheKey = "PaymentSettings";
		private const string OnErrorMessage = "Payment Method not found or unknown";

		public PaymentService(string url,
            Func<string> sessionId,
            ICacheService cache,
            IIPAddressManager ipAddressManager,
            IPackageInfo packageInfo,
            ILogger logger,
			IConnectivityService connectivityService)
        {
			_logger = logger;
			_packageInfo = packageInfo;
            _baseUrl = url;
		    _sessionId = sessionId;
		    _cache = cache;
			_ipAddressManager = ipAddressManager;
			_connectivityService = connectivityService;
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
		        var serviceClient = Mvx.Resolve<ConfigurationClientService>();

                _cachedSettings = await serviceClient.GetPaymentSettings().ConfigureAwait(false);
                _cache.Set(PaymentSettingsCacheKey, _cachedSettings);
                _client = GetClient(_cachedSettings);
            }
		    catch (Exception)
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

		public Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, string nameOnCard, DateTime expiryDate, string cvv, string kountSessionId, string zipCode, Account account)
        {
			return GetClient().Tokenize(creditCardNumber, nameOnCard, expiryDate, cvv, kountSessionId, zipCode, account);
        }

        public Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken)
        {
			return GetClient().ForgetTokenizedCard(cardToken);
        }

		public Task<BasePaymentResponse> ValidateTokenizedCard(CreditCardDetails creditCard, string cvv, string kountSessionId, Account account)
		{
			return GetClient().ValidateTokenizedCard(creditCard, cvv, kountSessionId, account);
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
			return await new PairingServiceClient(_baseUrl, _sessionId(), _packageInfo, _connectivityService)
                .Unpair(orderId);
        }

        public async Task<bool> UpdateAutoTip(Guid orderId, int autoTipPercentage)
        {
			return await new PairingServiceClient(_baseUrl, _sessionId(), _packageInfo, _connectivityService)
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
					return new BraintreeServiceClient(_baseUrl, _sessionId(), settings.BraintreeClientSettings.ClientKey, _packageInfo, _connectivityService);

                case PaymentMethod.RideLinqCmt:
                case PaymentMethod.Cmt:
					return new CmtPaymentClient(_baseUrl, _sessionId(), settings.CmtPaymentSettings, _ipAddressManager, _packageInfo, _logger, _connectivityService);

                case PaymentMethod.Moneris:
					return new MonerisServiceClient(_baseUrl, _sessionId(), settings.MonerisPaymentSettings, _packageInfo, _logger, _connectivityService);

                case PaymentMethod.Fake:
                    return new FakePaymentClient();

				default:
					return null;
            }
        }

		public async Task<GenerateClientTokenResponse> GenerateClientTokenResponse()
		{
			try
			{
				return await GetClient().GenerateClientTokenResponse();
			}
			catch(NotSupportedException)
			{
				Logger.LogMessage("Generate token only supported for Braintree");
			}

			return new GenerateClientTokenResponse();
		}

	    public async Task<TokenizedCreditCardResponse> AddPaymentMethod(string nonce, PaymentMethods method, Guid? creditCardId, string cardholderName = null)
	    {
            try
            {
                return await GetClient().AddPaymentMethod(nonce, method, creditCardId, cardholderName);
            }
            catch (NotSupportedException)
            {
                Logger.LogMessage("Generate token only supported for Braintree");
            }

            return new TokenizedCreditCardResponse()
            {
                IsSuccessful = false
            };
        }
    }
}