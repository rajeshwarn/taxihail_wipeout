using System;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using Cirrious.CrossCore;
using PaypalSdkDroid.Payments;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class PayPalConfigurationService : IPayPalConfigurationService
    {
        private static string _clientId;
        private static string _environment;

        public void InitializeService(PayPalClientSettings payPalSettings)
        {
            _clientId = payPalSettings.IsSandbox
                ? payPalSettings.SandboxCredentials.ClientId
                : payPalSettings.Credentials.ClientId;

            _environment = payPalSettings.IsSandbox
                ? PayPalConfiguration.EnvironmentSandbox
                : PayPalConfiguration.EnvironmentProduction;
        }

        public object GetConfiguration()
        {
            var settings = Mvx.Resolve<IAppSettings>().Data;
            var localize = Mvx.Resolve<ILocalization>();

            var baseUri = settings.ServiceUrl.Replace("api/", string.Empty);

            var configuration = new PayPalConfiguration();

            configuration.AcceptCreditCards(false);
            configuration.LanguageOrLocale(localize.CurrentLanguage);
            configuration.MerchantName(settings.TaxiHail.ApplicationName);
            configuration.MerchantPrivacyPolicyUri(Android.Net.Uri.Parse(string.Format("{0}/company/privacy", baseUri)));
            configuration.MerchantUserAgreementUri(Android.Net.Uri.Parse(string.Format("{0}/company/termsandconditions", baseUri)));
            configuration.Environment(_environment);
            configuration.ClientId(_clientId);
            
            return configuration;
        }
    }
}