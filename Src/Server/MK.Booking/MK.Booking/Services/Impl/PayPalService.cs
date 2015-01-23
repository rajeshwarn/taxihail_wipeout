using System;
using System.Net;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Resources;
using apcurium.MK.Common.Diagnostic;
using Infrastructure.Messaging;
using PayPal.Api;

namespace apcurium.MK.Booking.Services.Impl
{
    public class PayPalService
    {
        private readonly IServerSettings _serverSettings;
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _accountDao;
        private readonly ILogger _logger;

        public PayPalService(IServerSettings serverSettings, ICommandBus commandBus, IAccountDao accountDao, ILogger logger)
        {
            _serverSettings = serverSettings;
            _commandBus = commandBus;
            _accountDao = accountDao;
            _logger = logger;
        }

        public BasePaymentResponse LinkAccount(Guid accountId, string authCode, string metadataId)
        {
            try
            {
                var account = _accountDao.FindById(accountId);
                if (account == null)
                {
                    throw new Exception("Account not found.");
                }

                var authorizationCodeParameters = new CreateFromAuthorizationCodeParameters();
                authorizationCodeParameters.setClientId(GetClientId());
                authorizationCodeParameters.setClientSecret(GetSecret());
                authorizationCodeParameters.SetCode(authCode);

                // Get refresh and access tokens
                var tokenInfo = Tokeninfo.CreateFromAuthorizationCodeForFuturePayments(GetAPIContext(), authorizationCodeParameters);

                //test
                var accessTokenParameters = new CreateFromRefreshTokenParameters();
                accessTokenParameters.SetRefreshToken(tokenInfo.refresh_token);

                var to = new Tokeninfo().CreateFromRefreshToken(GetAPIContext(), accessTokenParameters);
                if (to != null)
                {
                    
                }

                _commandBus.Send(new LinkPayPalAccount
                {
                    AccountId = accountId,
                    RefreshToken = tokenInfo.refresh_token
                });

                return new BasePaymentResponse
                {
                    IsSuccessful = true,
                    Message = "Success"
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage("PayPal: LinkAccount error");
                _logger.LogError(e);
                return new BasePaymentResponse
                {
                    IsSuccessful = false,
                    Message = e.Message
                };
            }
        }

        public BasePaymentResponse UnlinkAccount(Guid accountId)
        {
            try
            {
                var account = _accountDao.FindById(accountId);
                if (account == null)
                {
                    throw new Exception("Account not found.");
                }

                _commandBus.Send(new UnlinkPayPalAccount { AccountId = accountId });

                return new BasePaymentResponse
                {
                    IsSuccessful = true,
                    Message = "Success"
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage("PayPal: LinkAccount error");
                _logger.LogError(e);
                return new BasePaymentResponse
                {
                    IsSuccessful = false,
                    Message = e.Message
                };
            }
        }

        private string GetClientId()
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();

            return paymentSettings.PayPalClientSettings.IsSandbox
                    ? paymentSettings.PayPalClientSettings.SandboxCredentials.ClientId
                    : paymentSettings.PayPalClientSettings.Credentials.ClientId;
        }

        private string GetSecret()
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();

            return paymentSettings.PayPalClientSettings.IsSandbox
                ? paymentSettings.PayPalServerSettings.SandboxCredentials.Secret
                : paymentSettings.PayPalServerSettings.Credentials.Secret; 
        }

        private string GetAccessToken()
        {
            // ###AccessToken
            // Retrieve the access token from
            // OAuthTokenCredential by passing in
            // ClientID and ClientSecret
            // It is not mandatory to generate Access Token on a per call basis.
            // Typically the access token can be generated once and
            // reused within the expiry window                
            return new OAuthTokenCredential(GetClientId(), GetSecret()).GetAccessToken();
        }

        // Returns APIContext object
        public APIContext GetAPIContext(string accessToken = "", Guid? orderId = null)
        {
            // ### Api Context
            // Pass in a `APIContext` object to authenticate 
            // the call and to send a unique request id 
            // (that ensures idempotency). The SDK generates
            // a request id if you do not pass one explicitly.

            if (orderId.HasValue)
            {
                return new APIContext(string.IsNullOrEmpty(accessToken) ? GetAccessToken() : accessToken, orderId.ToString());
            }
            return new APIContext(string.IsNullOrEmpty(accessToken) ? GetAccessToken() : accessToken);
        }
    }
}
