using System;
using System.Collections.Generic;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;
using PayPal.Api;

namespace apcurium.MK.Booking.Services.Impl
{
    [Obsolete("Kept for legacy support (order still in progress during update), use Braintree vZero instead")]
    public class BasePayPalService
    {
        private readonly ServerPaymentSettings _serverPaymentSettings;
        private readonly IAccountDao _accountDao;

        public BasePayPalService(ServerPaymentSettings serverPaymentSettings, IAccountDao accountDao)
        {
            _serverPaymentSettings = serverPaymentSettings;
            _accountDao = accountDao;
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
            return _serverPaymentSettings.PayPalClientSettings.IsSandbox
                    ? _serverPaymentSettings.PayPalClientSettings.SandboxCredentials.ClientId
                    : _serverPaymentSettings.PayPalClientSettings.Credentials.ClientId;
        }

        protected string GetSecret()
        {
            return _serverPaymentSettings.PayPalClientSettings.IsSandbox
                ? _serverPaymentSettings.PayPalServerSettings.SandboxCredentials.Secret
                : _serverPaymentSettings.PayPalServerSettings.Credentials.Secret;
        }

        protected string GetMode()
        {
            return _serverPaymentSettings.PayPalClientSettings.IsSandbox
                ? BaseConstants.SandboxMode
                : BaseConstants.LiveMode;
        }

        protected string GetAccessToken()
        {
            return new OAuthTokenCredential(GetClientId(), GetSecret(), GetConfig()).GetAccessToken();
        }

        protected string GetAccessToken(Guid accountId)
        {
            var encodedRefreshToken = _accountDao.GetPayPalEncryptedRefreshToken(accountId);
            if (!encodedRefreshToken.HasValue())
            {
                throw new ArgumentNullException(string.Format("Refresh token not found for account: {0}", accountId));
            }

            var tokenInfo = new Tokeninfo { refresh_token = CryptoService.Decrypt(encodedRefreshToken) };
            var tokenResult = tokenInfo.CreateFromRefreshToken(GetAPIContext(), new CreateFromRefreshTokenParameters());

            return string.Format("{0} {1}", tokenResult.token_type, tokenResult.access_token);
        }

        protected Dictionary<string, string> GetConfig()
        {
            var config = ConfigManager.Instance.GetProperties();

            config.Add(BaseConstants.ApplicationModeConfig, GetMode());
            config.Add(BaseConstants.ClientId, GetClientId());
            config.Add(BaseConstants.ClientSecret, GetSecret());

            return config;
        }
    }
}
