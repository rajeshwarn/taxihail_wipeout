using System;
using System.Collections.Generic;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using PayPal.Api;

namespace apcurium.MK.Booking.Services.Impl
{
    public class BasePayPalService
    {
        private readonly IServerSettings _serverSettings;
        private readonly IAccountDao _accountDao;

        public BasePayPalService(IServerSettings serverSettings, IAccountDao accountDao)
        {
            _serverSettings = serverSettings;
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
