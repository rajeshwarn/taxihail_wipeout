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
using apcurium.MK.Common.Enumeration.PayPal;
using apcurium.MK.Common.Resources;
using Infrastructure.Messaging;
using PayPal;
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

        public PreAuthorizePaymentResponse PreAuthorize(Guid accountId, Guid orderId, string email, decimal amountToPreAuthorize, bool isReAuth = false)
        {
            var message = string.Empty;
            var transactionId = string.Empty;
            var preAuthAmount = amountToPreAuthorize;

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
                    preAuthAmount = amount;

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

                    var refreshToken = _accountDao.GetPayPalEncodedRefreshToken(accountId);
                    if (!refreshToken.HasValue())
                    {
                        throw new Exception("Account has no PayPal refresh token");
                    }

                    var accessToken = GetAccessToken(accountId);

                    var createdPayment = futurePayment.Create(GetAPIContext(accessToken, isReAuth ? Guid.NewGuid() : orderId));
                    transactionId = createdPayment.transactions[0].related_resources[0].authorization.id;

                    switch (createdPayment.state)
                    {
                        case PaymentStates.Approved:
                            isSuccessful = true;
                            break;
                        case PaymentStates.Created:
                        case PaymentStates.Pending:
                            message = string.Format("Authorization state was {0}", createdPayment.state);
                            break;
                        case PaymentStates.Failed:
                        case PaymentStates.Canceled:
                        case PaymentStates.Expired:
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

                if (isSuccessful && !isReAuth)
                {
                    var paymentId = Guid.NewGuid();
                    _commandBus.Send(new InitiateCreditCardPayment
                    {
                        PaymentId = paymentId,
                        Amount = preAuthAmount,
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
            catch (Exception ex)
            {
                var exceptionMessage = ex.Message;

                var paymentException = ex as PaymentsException;
                if (paymentException != null && paymentException.Details != null)
                {
                    exceptionMessage = paymentException.Details.message;
                }

                _logger.LogMessage(string.Format("Error during preauthorization (validation of the PayPal account) for client {0}: {1} - {2}",
                    email, message + exceptionMessage, paymentException ?? ex));
                _logger.LogError(paymentException ?? ex);

                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = false,
                    Message = message
                };
            }
        }
        private PreAuthorizePaymentResponse ReAuthorizeIfNecessary(Guid accountId, Guid orderId, decimal preauthAmount, decimal amount)
        {
            if (amount <= preauthAmount)
            {
                return new PreAuthorizePaymentResponse
                {
                    IsSuccessful = true
                };
            }

            _logger.LogMessage(string.Format("Re-Authorizing order {0} because it exceeded the original pre-auth amount ", orderId));
            _logger.LogMessage(string.Format("Voiding original Pre-Auth of {0}", preauthAmount));

            VoidPreAuthorization(orderId);

            var account = _accountDao.FindById(accountId);

            _logger.LogMessage(string.Format("Re-Authorizing order for amount of {0}", amount));

            return PreAuthorize(accountId, orderId, account.Email, amount, true);
        }

        public CommitPreauthorizedPaymentResponse CommitPayment(Guid orderId, decimal preauthAmount, decimal amount, decimal meterAmount, decimal tipAmount, string transactionId)
        {
            var order = _orderDao.FindById(orderId);
            var accessToken = GetAccessToken(order.AccountId);
            var apiContext = GetAPIContext(accessToken, orderId);

            var updatedTransactionId = transactionId;

            try
            {
                var authResponse = ReAuthorizeIfNecessary(order.AccountId, orderId, preauthAmount, amount);
                if (!authResponse.IsSuccessful)
                {
                    return new CommitPreauthorizedPaymentResponse
                    {
                        IsSuccessful = false,
                        TransactionId = updatedTransactionId,
                        Message = string.Format("PayPal Re-Auth of amount {0} failed.", amount)
                    };
                }

                // If we need to re-authorize, we have to update the id since it's another transaction
                if (authResponse.TransactionId.HasValue())
                {
                    updatedTransactionId = authResponse.TransactionId;
                }

                var authorization = Authorization.Get(apiContext, updatedTransactionId);

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

                var isSuccessful = responseCapture.state == PaymentStates.Pending
                    || responseCapture.state == PaymentStates.Completed;

                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = isSuccessful,
                    AuthorizationCode = responseCapture.id,
                    TransactionId = updatedTransactionId
                };
            }
            catch (Exception ex)
            {
                var exceptionMessage = ex.Message;

                var exception = ex as PaymentsException;
                if (exception != null && exception.Details != null)
                {
                    exceptionMessage = exception.Details.message;
                }

                return new CommitPreauthorizedPaymentResponse
                {
                    IsSuccessful = false,
                    TransactionId = updatedTransactionId,
                    Message = string.Format("PayPal commit of amount {0} failed. {1}", amount, exceptionMessage)
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

                if (authorization.state == AuthorizationStates.Authorized)
                {
                    // Void preauth
                    var cancellationResult = authorization.Void(apiContext);
                    if (cancellationResult.state == AuthorizationStates.Voided)
                    {
                        isTransactionCancelled = true;
                    }
                }
                else if (authorization.state == AuthorizationStates.Captured || authorization.state == AuthorizationStates.PartiallyCaptured)
                {
                    // Refund transaction
                    var paymentDetails = _paymentDao.FindByOrderId(orderId);

                    var captureResponse = Capture.Get(apiContext, paymentDetails.AuthorizationCode);

                    var refund = new Refund
                    {
                        amount = new Amount
                        {
                            currency = captureResponse.amount.currency,
                            total = captureResponse.amount.total
                        }
                    };

                    var refundResult = captureResponse.Refund(apiContext, refund);
                    if (refundResult.state != PaymentStates.Failed)
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
                var exceptionMessage = ex.Message;

                var exception = ex as PaymentsException;
                if (exception != null && exception.Details != null)
                {
                    exceptionMessage = exception.Details.message;
                }

                message = "The transaction couldn't be cancelled " + exceptionMessage;
                throw;
            }
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
