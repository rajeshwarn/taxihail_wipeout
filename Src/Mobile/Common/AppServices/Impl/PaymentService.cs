#if SOCIAL_NETWORKS
using SocialNetworks.Services;
#endif
using System;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Booking.Api.Client.Payments.Braintree;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;

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
        ILogger _logger;

        public PaymentService(string url, string sessionId, IConfigurationManager configurationManager, ICacheService cache, ILogger logger)
        {
            _logger = logger;
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
            _cache.Set(orderId + PayedCacheSuffix, amount.ToString());            
        }

        public IPaymentServiceClient GetClient()
        {
            const string onErrorMessage = "Payment Method not found or unknown";

            var settings = _configurationManager.GetPaymentSettings();



            switch (settings.PaymentMode)
            {
                case PaymentMethod.Braintree:
                    return new BraintreeServiceClient(_baseUrl, _sessionId, settings.BraintreeClientSettings.ClientKey, TinyIoCContainer.Current.Resolve<IPackageInfo>().UserAgent);;

                case PaymentMethod.Cmt:
                    return new CmtPaymentClient(_baseUrl, _sessionId, settings.CmtPaymentSettings, _logger, TinyIoCContainer.Current.Resolve<IPackageInfo>().UserAgent);

                case PaymentMethod.Fake:
                    return new FakePaymentClient();
                default:
                    throw new Exception(onErrorMessage);
            }
        }

        public void ResendConfirmationToDriver(Guid orderId)
        {
            GetClient().ResendConfirmationToDriver(orderId);
        }

        public TokenizedCreditCardResponse Tokenize(string creditCardNumber, DateTime expiryDate, string cvv)
        {
            return GetClient().Tokenize(creditCardNumber, expiryDate, cvv);
        }

        public DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken)
        {
            return GetClient().ForgetTokenizedCard(cardToken);
        }

        public PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, double meterAmount, double tipAmount, Guid orderId)
        {
            return GetClient().PreAuthorize(cardToken, amount, meterAmount, tipAmount, orderId);
        }

        public CommitPreauthorizedPaymentResponse CommitPreAuthorized(string transactionId)
        {
            return GetClient().CommitPreAuthorized(transactionId);
        }

        public CommitPreauthorizedPaymentResponse PreAuthorizeAndCommit(string cardToken, double amount, double meterAmount, double tipAmount, Guid orderId)
        {
            return GetClient().PreAuthorizeAndCommit(cardToken, amount, meterAmount, tipAmount, orderId);
        }   
    }
}


