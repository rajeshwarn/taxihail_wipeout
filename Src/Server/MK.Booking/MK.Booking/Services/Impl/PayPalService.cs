using System;
using System.Collections.Generic;
using System.Globalization;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Resources;
using Infrastructure.Messaging;
using PayPal.Api;
using RestSharp.Extensions;

namespace apcurium.MK.Booking.Services.Impl
{
    public class PayPalService : BasePayPalService
    {
        private readonly IServerSettings _serverSettings;
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly ILogger _logger;
        private readonly IPairingService _pairingService;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly Resources.Resources _resources;

        public PayPalService(IServerSettings serverSettings,
            ICommandBus commandBus,
            IAccountDao accountDao,
            IOrderDao orderDao,
            ILogger logger,
            IPairingService pairingService,
            IOrderPaymentDao paymentDao) : base(serverSettings, accountDao)
        {
            _serverSettings = serverSettings;
            _commandBus = commandBus;
            _accountDao = accountDao;
            _orderDao = orderDao;
            _logger = logger;
            _pairingService = pairingService;
            _paymentDao = paymentDao;

            _resources = new Resources.Resources(serverSettings);
        }

        public BasePaymentResponse LinkAccount(Guid accountId, string authCode)
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

                // Store access token securely
                _commandBus.Send(new LinkPayPalAccount
                {
                    AccountId = accountId,
                    RefreshToken = CryptoService.Encrypt(tokenInfo.refresh_token)
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

        public PairingResponse Pair(Guid orderId, int? autoTipPercentage)
        {
            try
            {
                _pairingService.Pair(orderId, null, autoTipPercentage);

                return new PairingResponse
                {
                    IsSuccessful = true,
                    Message = "Success"
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e);
                return new PairingResponse
                {
                    IsSuccessful = false,
                    Message = e.Message
                };
            }
        }

        public BasePaymentResponse Unpair(Guid orderId)
        {
            _pairingService.Unpair(orderId);

            return new BasePaymentResponse
            {
                IsSuccessful = true,
                Message = "Success"
            };
        }

        public PreAuthorizePaymentResponse PreAuthorize(Guid accountId, Guid orderId, string email, decimal amountToPreAuthorize, string metadataId = "")
        {
            var message = string.Empty;
            var transactionId = string.Empty;

            try
            {
                var isSuccessful = false;

                if (amountToPreAuthorize > 0)
                {
                    var account = _accountDao.FindById(accountId);
                    var regionName = _serverSettings.ServerData.PayPalRegionInfoOverride;
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
                                description = regionName.HasValue()
                                    ? string.Format("order: {0}", orderId)
                                    : string.Format(_resources.Get("PaymentItemDescription", account.Language), orderId, amountToPreAuthorize)
                            }
                        }
                    };

                    var refreshToken = _accountDao.GetPayPalRefreshToken(accountId);
                    if (!refreshToken.HasValue())
                    {
                        throw new Exception("Account has no PayPal refresh token");
                    }

                    var accessToken = GetAccessToken(accountId);

                    var createdPayment = futurePayment.Create(GetAPIContext(accessToken, orderId)/*, metadataId*/);
                    transactionId = createdPayment.transactions[0].related_resources[0].authorization.id;

                    switch (createdPayment.state)
                    {
                        case "approved":
                            isSuccessful = true;
                            break;
                        case "created":
                        case "pending":
                            // what is that supposed to mean?
                            message = string.Format("Authorization state was {0}", createdPayment.state);
                            break;
                        case "failed":
                        case "canceled":
                        case "expired":
                            message = string.Format("Authorization state was {0}", createdPayment.state);
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

        public void VoidPreAuthorization(Guid orderId)
        {
            var message = string.Empty;
            try
            {
                var paymentDetail = _paymentDao.FindByOrderId(orderId);
                if (paymentDetail == null)
                {
                    // nothing to void
                    return;
                }

                Void(orderId, paymentDetail.TransactionId, ref message);
            }
            catch (Exception ex)
            {
                _logger.LogMessage("Can't cancel PayPal preauthorization");
                _logger.LogError(ex);
                message = message + ex.Message;
            }
            finally
            {
                _logger.LogMessage(message);
            }
        }

        public void VoidTransaction(Guid orderId, string transactionId, ref string message)
        {
            Void(orderId, transactionId, ref message);
        }

        private void Void(Guid orderId, string transactionId, ref string message)
        {
            try
            {
                var order = _orderDao.FindById(orderId);
                var accessToken = GetAccessToken(order.AccountId);
                var apiContext = GetAPIContext(accessToken, orderId);
                bool isTransactionCancelled = false;

                var authorization = Authorization.Get(apiContext, transactionId);

                if (authorization.state == "authorized")
                {
                    var cancellationResult = authorization.Void(apiContext);
                    if (cancellationResult.state == "voided")
                    {
                        isTransactionCancelled = true;
                    }
                }
                else if (authorization.state == "captured" || authorization.state == "partially_captured")
                {
                    // TODO PayPal test
                    var payment = Payment.Get(apiContext, authorization.parent_payment);
                    var captureResponse = payment.transactions[0].related_resources[0].capture;

                    var refund = new Refund
                    {
                        amount = new Amount
                        {
                            currency = captureResponse.amount.currency,
                            total = captureResponse.amount.total
                        }
                    };
                    var refundResult = captureResponse.Refund(apiContext, refund);
                    if (refundResult.state != "failed")
                    {
                        isTransactionCancelled = true;
                    }
                }

                if (!isTransactionCancelled)
                {
                    throw new Exception(string.Format("transaction {0} status {1}, can't cancel it",
                        transactionId, authorization.state));
                }

                message = message + " The transaction has been cancelled.";
            }
            catch (Exception ex)
            {
                message = "The transaction couldn't be cancelled" + ex.Message;
                throw;
            }
        }

        public CommitPreauthorizedPaymentResponse CommitPayment(Guid orderId, decimal amount, decimal meterAmount, decimal tipAmount, string authorizationId)
        {
            var apiContext = GetAPIContext(string.Empty, orderId);

            var authorization = Authorization.Get(apiContext, authorizationId);

            var capture = new Capture
            {
                amount = new Amount
                {
                    currency = authorization.amount.currency,
                    total = amount.ToString(CultureInfo.InvariantCulture)
                },
                is_final_capture = true
            };

            var responseCapture = authorization.Capture(apiContext, capture);

            var isSuccessful = responseCapture.state == "pending" || responseCapture.state == "completed";

            // TODO PayPal test
            return new CommitPreauthorizedPaymentResponse
            {
                IsSuccessful = isSuccessful,
                AuthorizationCode = responseCapture.id,
                TransactionId = authorizationId
            };
        }

        public bool TestCredentials(PayPalClientCredentials payPalClientSettings, PayPalServerCredentials payPalServerSettings, bool isSandbox)
        {
            try
            {
                var payPalMode = isSandbox ? BaseConstants.SandboxMode : BaseConstants.LiveMode;

                var config = new Dictionary<string, string> { { BaseConstants.ApplicationModeConfig, payPalMode } };

                var tokenCredentials = new OAuthTokenCredential(payPalClientSettings.ClientId, payPalServerSettings.Secret, config);
                var accessToken = tokenCredentials.GetAccessToken();

                return accessToken.HasValue();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
