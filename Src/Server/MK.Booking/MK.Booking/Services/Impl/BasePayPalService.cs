using System;
using System.Collections.Generic;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using PayPal.Api;

namespace apcurium.MK.Booking.Services.Impl
{
    public class BasePayPalService
    {
        private readonly IServerSettings _serverSettings;

        public BasePayPalService(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
        }

        protected APIContext GetAPIContext(string accessToken = "", Guid? orderId = null)
        {
            // Pass in a `APIContext` object to authenticate 
            // the call and to send a unique request id 
            // (that ensures idempotency). The SDK generates
            // a request id if you do not pass one explicitly.
            APIContext apiContext;

            if (accessToken.HasValue() && orderId.HasValue)
            {
                apiContext = new APIContext(accessToken, orderId.ToString());
            }
            else if (accessToken.HasValue())
            {
                apiContext = new APIContext(accessToken);
            }
            else
            {
                apiContext = new APIContext();
            }

            apiContext.Config = GetConfig();

            return apiContext;
        }

        protected string GetClientId()
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();

            return paymentSettings.PayPalClientSettings.IsSandbox
                    ? paymentSettings.PayPalClientSettings.SandboxCredentials.ClientId
                    : paymentSettings.PayPalClientSettings.Credentials.ClientId;
        }

        protected string GetSecret()
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();

            return paymentSettings.PayPalClientSettings.IsSandbox
                ? paymentSettings.PayPalServerSettings.SandboxCredentials.Secret
                : paymentSettings.PayPalServerSettings.Credentials.Secret;
        }

        protected string GetMode()
        {
            return _serverSettings.GetPaymentSettings().PayPalClientSettings.IsSandbox
                ? BaseConstants.SandboxMode
                : BaseConstants.LiveMode;
        }

        protected string GetAccessToken()
        {
            // Retrieve the access token from
            // OAuthTokenCredential by passing in
            // ClientID and ClientSecret
            // It is not mandatory to generate Access Token on a per call basis.
            // Typically the access token can be generated once and
            // reused within the expiry window
            return new OAuthTokenCredential(GetClientId(), GetSecret(), GetConfig()).GetAccessToken();
        }

        protected Dictionary<string, string> GetConfig()
        {
            var payPalConfig = ConfigManager.Instance.GetProperties();

            payPalConfig.Add(BaseConstants.ApplicationModeConfig, GetMode());
            payPalConfig.Add(BaseConstants.ClientId, GetClientId());
            payPalConfig.Add(BaseConstants.ClientSecret, GetSecret());

            return payPalConfig;
        }
    }
}
