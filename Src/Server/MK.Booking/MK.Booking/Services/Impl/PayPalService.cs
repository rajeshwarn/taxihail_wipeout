using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Resources;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using Braintree;
using Infrastructure.Messaging;
using PayPal.Api;
using RestSharp.Extensions;
using Authorization = PayPal.Api.Authorization;
using Transaction = PayPal.Api.Transaction;

namespace apcurium.MK.Booking.Services.Impl
{
    public class PayPalService
    {
        private readonly IServerSettings _serverSettings;
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _accountDao;
        private readonly ILogger _logger;
        private Resources.Resources _resources;

        public PayPalService(IServerSettings serverSettings, ICommandBus commandBus, IAccountDao accountDao, ILogger logger)
        {
            _serverSettings = serverSettings;
            _commandBus = commandBus;
            _accountDao = accountDao;
            _logger = logger;

            _resources = new Resources.Resources(serverSettings);
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

        public PreAuthorizePaymentResponse PreAuthorize(Guid accountId, Guid orderId, string email, decimal amountToPreAuthorize)
        {
            var message = string.Empty;
            var transactionId = string.Empty;

            try
            {
                var isSuccessful = false;

                if (amountToPreAuthorize > 0)
                {
                    var conversionRate = _serverSettings.ServerData.PayPalConversionRate;
                    _logger.LogMessage("PayPal Conversion Rate: {0}", conversionRate);
                    var amount = Math.Round(amountToPreAuthorize * conversionRate, 2);

                    var futurePayment = new FuturePayment
                    {
                        intent = "authorize",
                        payer = new Payer
                        {
                            payment_method = "paypal"
                        },
                        transactions = new List<Transaction>
                        {
                            new Transaction
                            {
                                amount = new Amount
                                {
                                    currency = conversionRate != 1 
                                        ? CurrencyCodes.Main.UnitedStatesDollar 
                                        : _resources.GetCurrencyCode(),
                                    total = amount.ToString(CultureInfo.InvariantCulture)
                                },
                                description = "preauthorization"
                            }
                        }
                    };

                    var refreshToken = _accountDao.GetPayPalRefreshToken(accountId);
                    if (!refreshToken.HasValue())
                    {
                        throw new Exception("Account has no PayPal refresh token");
                    }

                    var accessTokenParameters = new CreateFromRefreshTokenParameters();
                    accessTokenParameters.SetRefreshToken(refreshToken);

                    var tokenInfo = new Tokeninfo().CreateFromRefreshToken(GetAPIContext(), accessTokenParameters);
                    
                    var createdPayment = futurePayment.Create(GetAPIContext(tokenInfo.access_token, orderId), "");
                    var authorization = createdPayment.transactions[0].related_resources[0].authorization;
                    transactionId = authorization.id;
                    
                    switch (authorization.state)
                    {
                        case "approved":
                            isSuccessful = true;
                            break;
                        case "created":
                        case "pending":
                            // what is that supposed to mean?
                            message = string.Format("Authorization state was {0}", authorization.state);
                            break;
                        case "failed":
                        case "canceled":
                        case "expired":
                            message = string.Format("Authorization state was {0}", authorization.state);
                            break;
                    }
                }
                else
                {
                    // if we're preauthorizing $0, we skip the preauth with payment provider
                    // but we still send the InitiateCreditCardPayment command
                    // this should never happen in the case of a real preauth (hence the minimum of $50)
                    isSuccessful = true;
                }

                if (isSuccessful)
                {
                    var paymentId = Guid.NewGuid();
                    _commandBus.Send(new InitiateCreditCardPayment
                    {
                        PaymentId = paymentId,
                        Amount = 0,
                        Meter = 0,
                        Tip = 0,
                        TransactionId = transactionId,
                        OrderId = orderId,
                        Provider = PaymentProvider.PayPal,
                        IsNoShowFee = false
                    });
                }

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = isSuccessful,
                    Message = message,
                    TransactionId = transactionId
                };
            }
            catch (Exception e)
            {
                _logger.LogMessage(string.Format("Error during preauthorization (validation of the PayPal account) for client {0}: {1} - {2}", email, message, e));
                _logger.LogError(e);

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = false,
                    Message = message
                };
            }
        }

        private void Capture(Guid orderId, string authorizationId)
        {
            var apiContext = GetAPIContext("", orderId);

            var authorization = Authorization.Get(apiContext, authorizationId);

            var capture = new Capture
            {
                amount = new Amount
                {
                    currency = "USD",
                    total = "4.54"
                },
                is_final_capture = true
            };

            var responseCapture = authorization.Capture(apiContext, capture);
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
