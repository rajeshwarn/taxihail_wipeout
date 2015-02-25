using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using Cirrious.CrossCore;
using Foundation;
using PaypalSdkTouch.Unified;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class PayPalConfigurationService : IPayPalConfigurationService
    {
        public void InitializeService(PayPalClientSettings payPalSettings)
        {
            PayPalMobile.WithClientIds(
                (NSString)payPalSettings.Credentials.ClientId,
                (NSString)payPalSettings.SandboxCredentials.ClientId);

            PayPalMobile.PreconnectWithEnvironment(payPalSettings.IsSandbox
                ? PayPalMobile.PayPalEnvironmentSandbox
                : PayPalMobile.PayPalEnvironmentProduction);
        }

        public object GetConfiguration()
        {
            var settings = Mvx.Resolve<IAppSettings>().Data;
            var localize = Mvx.Resolve<ILocalization>();

            var baseUri = settings.ServiceUrl.Replace("api/", string.Empty);

            return new PayPalConfiguration
            {
                AcceptCreditCards = false,
                LanguageOrLocale = (NSString)localize.CurrentLanguage,
                MerchantName = (NSString)settings.TaxiHail.ApplicationName,
                MerchantPrivacyPolicyURL = new NSUrl(string.Format("{0}/company/privacy", baseUri)),
                MerchantUserAgreementURL = new NSUrl(string.Format("{0}/company/termsandconditions", baseUri)),
                DisableBlurWhenBackgrounding = true
            };
        }
    }
}