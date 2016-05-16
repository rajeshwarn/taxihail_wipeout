using System;
using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common;

#if CLIENT
using MK.Common.Exceptions;
#else
using apcurium.MK.Booking.Api.Client.Extensions;
using ServiceStack.ServiceClient.Web;
#endif
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using CMTPayment;
using CMTPayment.Tokenize;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments
{
    /// <summary>
    ///     The Tokenize resource provides developers the ability to create a token in place of
    ///     cardholder data, update tokenized data and delete a token. The token does not use the
    ///     cardholder data to create the token so there is no way to get the cardholder information
    ///     with just the token alone
    /// </summary>
    public class CmtPaymentClient : BaseServiceClient, IPaymentServiceClient
    {
        private readonly IIPAddressManager _ipAddressManager;
        private readonly ILogger _logger;
        private readonly int _validateTokenThreshold;


        public CmtPaymentClient(string baseUrl, string sessionId, CmtPaymentSettings cmtSettings, IIPAddressManager ipAddressManager, IPackageInfo packageInfo, ILogger logger, IConnectivityService connectivityService)
            : base(baseUrl, sessionId, packageInfo, connectivityService)
        {
            _ipAddressManager = ipAddressManager;
            _logger = logger;
            _validateTokenThreshold = (int)cmtSettings.TokenizeValidateFrequencyThresholdInHours;

            CmtPaymentServiceClient = new CmtPaymentServiceClient(cmtSettings, null, packageInfo, logger, connectivityService);
        }

        private CmtPaymentServiceClient CmtPaymentServiceClient { get; set; }

        public Task<TokenizedCreditCardResponse> Tokenize(string accountNumber, string nameOnCard, DateTime expiryDate, string cvv, string kountSessionId, string zipCode, Account account)
        {
            return Tokenize(CmtPaymentServiceClient, nameOnCard, accountNumber, expiryDate, cvv, kountSessionId, zipCode, account);
        }

        public async Task<BasePaymentResponse> ValidateTokenizedCard(CreditCardDetails creditCard, string cvv, string kountSessionId, Account account)
        {
            try
            {
                if (ValidationRequired(creditCard))
                {

                    var request = new TokenizeValidateRequest
                    {
                        Token = creditCard.Token,
                        Cvv = cvv,
                        SessionId = kountSessionId,
                        Email = account.Email,
                        BillingFullName = creditCard.NameOnCard,
                        CustomerIpAddress = _ipAddressManager.GetIPAddress()
                    };

                    if (creditCard.ZipCode.HasValue())
                    {
                        request.ZipCode = creditCard.ZipCode;
                    }

                    var response = await CmtPaymentServiceClient.PostAsync(request);

                    if (response.ResponseCode == 1)
                    {
                        // the token validate call was successful, so update the date/time for the credit card.
                        creditCard.LastTokenValidateDateTime = DateTime.Now;
                        await UpdateCreditCard(creditCard);
                    }

                    return new BasePaymentResponse
                    {
                        IsSuccessful = response.ResponseCode == 1,
                        Message = response.ResponseMessage
                    };

                }
                else
                {
                    return new BasePaymentResponse
                    {
                        IsSuccessful = true,
                        Message = "Approved"
                    };
                }

            }
            catch(Exception e)
            {
                _logger.Maybe(x => x.LogMessage("Error during card validation"));
                _logger.Maybe(x => x.LogError(e));

                var message = e.Message;
                var exception = e as AggregateException;
                if (exception != null)
                {
                    message = exception.InnerException.Message;
                }

                return new BasePaymentResponse
                {
                    IsSuccessful = false,
                    Message = message
                };
            }
        }


        /// <summary>
        /// This method should not remove CMT token in CMT payment service, according to ticket https://apcurium.atlassian.net/browse/MKTAXI-3225
        /// </summary>
        /// <param name="cardToken"></param>
        /// <returns></returns>
        public async Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken)
        {
            return new DeleteTokenizedCreditcardResponse { IsSuccessful = true };
        }

        public Task<OverduePayment> GetOverduePayment()
        {
            return Client.GetAsync<OverduePayment>("/account/overduepayment", logger: _logger);
        }

        public Task<SettleOverduePaymentResponse> SettleOverduePayment(string kountSessionId)
        {
            return Client.PostAsync(new SettleOverduePaymentRequest
            {
                KountSessionId = kountSessionId,
                CustomerIpAddress = _ipAddressManager.GetIPAddress()
            });
        }

        public Task<BasePaymentResponse> Unpair(Guid orderId)
        {
            return Client.PostAsync(new UnpairingForPaymentRequest
            {
                OrderId = orderId
            });
        }

        private async Task<TokenizedCreditCardResponse> Tokenize(CmtPaymentServiceClient cmtPaymentServiceClient, string nameOnCard,
            string accountNumber, DateTime expiryDate, string cvv, string kountSessionId, string zipCode, Account account)
        {
            try
            {
                var request = new TokenizeRequest
                {
                    AccountNumber = accountNumber,
                    ExpiryDate = expiryDate.ToString("yyMM", CultureInfo.InvariantCulture),
                    Cvv = cvv,
                    SessionId = kountSessionId,
                    Email = account.Email,
                    BillingFullName = nameOnCard,
                    CustomerId = account.Id.ToString(),
                    CustomerIpAddress = _ipAddressManager.GetIPAddress()
                };
                
                if(zipCode.HasValue())
                {
                    request.ZipCode = zipCode;
                }

                var response = await cmtPaymentServiceClient.PostAsync(request);

                return new TokenizedCreditCardResponse
                {
                    CardOnFileToken = response.CardOnFileToken,
                    IsSuccessful = response.ResponseCode == 1,
                    Message = response.ResponseMessage,
                    CardType = response.CardType,
                    LastFour = response.LastFour,
                };
            }
            catch(Exception e)
            {
                _logger.Maybe(x => x.LogMessage("Error during CMT tokenization"));

                var message = GetTokenizationErrorMessageFromCMT(e);
                if (message.HasValueTrimmed())
                {
                    return new TokenizedCreditCardResponse
                    {
                        IsSuccessful = false,
                        Message = message
                    };
                }

                // the exception is not a cmt error, log the exception
                _logger.Maybe(x => x.LogError(e));

                message = e.Message;
                var exception = e as AggregateException;
                if (exception != null)
                {
                    message = exception.InnerException.Message;
                }

                return new TokenizedCreditCardResponse
                {
                    IsSuccessful = false,
                    Message = message
                };
            }
        }

        private string GetTokenizationErrorMessageFromCMT(Exception e)
        {
            string message = null;

            var webServiceException = e as WebServiceException;
            if (webServiceException != null)
            {
                var cmtResponse = webServiceException.ResponseBody.FromJsonSafe<TokenizeResponse>();
                if (cmtResponse != null)
                {
                    message = cmtResponse.ResponseMessage;
                }
            }

            return message;
        }

        private static TokenizedCreditCardResponse TokenizeSyncForSettingsTest(CmtPaymentServiceClient cmtPaymentServiceClient, string accountNumber, DateTime expiryDate)
        {
            try
            {
                var response = cmtPaymentServiceClient.PostAsync(new TokenizeRequest
                {
                    AccountNumber = accountNumber,
                    ExpiryDate = expiryDate.ToString("yyMM", CultureInfo.InvariantCulture),
                    ValidateAccountInformation = false
                });

                response.Wait();

                return new TokenizedCreditCardResponse
                {
                    CardOnFileToken = response.Result.CardOnFileToken,
                    IsSuccessful = response.Result.ResponseCode == 1,
                    Message = response.Result.ResponseMessage,
                    CardType = response.Result.CardType,
                    LastFour = response.Result.LastFour
                };
            }
            catch (Exception e)
            {
                var message = e.Message;
                var exception = e as AggregateException;
                if (exception != null)
                {
                    message = exception.InnerException.Message;
                }

                return new TokenizedCreditCardResponse
                {
                    IsSuccessful = false,
                    Message = message
                };
            }
        }

        public static bool TestClient(CmtPaymentSettings serverPaymentSettings, string number, DateTime date, ILogger logger)
        {
            var cmtPaymentServiceClient = new CmtPaymentServiceClient(serverPaymentSettings, null, null, logger, null);
            var result = TokenizeSyncForSettingsTest(cmtPaymentServiceClient, number, date);
            return result.IsSuccessful;
        }

        /// <summary>
        /// Only perform ValidateTokenizedCard when it hasn't been done recently
        /// ARRO-0973
        /// Check configured threshold time interval value, 
        /// if timespan since last check is over the threshold value, allow the validation request to go through
        /// otherwise, just return the approved response to the caller, a successful check was done recently enough.
        /// Too many calls to the ValidateTokenizedCard were causing credit cards to be erronously flagged for fraudulant use
        /// </summary>
        /// <param name="creditCard">creditCard details</param>
        /// <returns>True if a validation request should be allowed through, flase otherwise</returns>
        private bool ValidationRequired(CreditCardDetails creditCard)
        {
            if (creditCard.LastTokenValidateDateTime == null)
                return true;

            // is the timespan since the last successful check greater than the configured value?
            TimeSpan span = DateTime.Now - (DateTime)creditCard.LastTokenValidateDateTime;
            int hoursRounded = (int)Math.Round(span.TotalHours);

            // if we are past the threshold, we need to validate again
            return (hoursRounded > _validateTokenThreshold);
        }

        public async Task<bool> UpdateCreditCard(CreditCardDetails creditCard)
        {
            try
            {
                //var req = "/account/creditcards";
                var req = string.Format("/account/creditcards/{0}", creditCard.CreditCardId);

                //var updateCreditCard = new AddOrUpdateCreditCard
                //{
                //            CreditCardCompany = creditCard.CreditCardCompany,
                //            CreditCardId = creditCard.CreditCardId,
                //            NameOnCard = creditCard.NameOnCard,
                //            Last4Digits = creditCard.Last4Digits,
                //            ExpirationMonth = creditCard.ExpirationMonth,
                //            ExpirationYear = creditCard.ExpirationYear,
                //            Token = creditCard.Token,
                //            Label = creditCard.Label.ToString(),
                //            ZipCode = creditCard.ZipCode,
                //            LastTokenValidateDateTime = creditCard.LastTokenValidateDateTime
                //};

                //await Client.PutAsync<string>(req, updateCreditCard, _logger);
                //await Client.PostAsync<string>(req, updateCreditCard, _logger);
                //CreditCardRequest request = new
                //{
                //        CreditCardCompany = creditCard.CreditCardCompany,
                //        CreditCardId = creditCard.CreditCardId,
                //        NameOnCard = creditCard.NameOnCard,
                //        Last4Digits = creditCard.Last4Digits,
                //        ExpirationMonth = creditCard.ExpirationMonth,
                //        ExpirationYear = creditCard.ExpirationYear,
                //        Token = creditCard.Token,
                //        Label = creditCard.Label.ToString(),
                //        ZipCode = creditCard.ZipCode,
                //        LastTokenValidateDateTime = creditCard.LastTokenValidateDateTime

                //}
                //return Client.PutAsync<string>(req, creditCard, logger: ILogger);

                // take the CreditCardDetails (which is a read model) and queue a command with the write model equivalent of the structure

                //var result = await Client.PostAsync(new AddOrUpdateCreditCard
                //{
                //    // do we need to set all properties?
                //    // can we update the credit card like this?
                //    CreditCardCompany = creditCard.CreditCardCompany,
                //    CreditCardId = creditCard.CreditCardId,
                //    NameOnCard = creditCard.NameOnCard,
                //    Last4Digits = creditCard.Last4Digits,
                //    ExpirationMonth = creditCard.ExpirationMonth,
                //    ExpirationYear = creditCard.ExpirationYear,
                //    Token = creditCard.Token,
                //    Label = creditCard.Label.ToString(),
                //    ZipCode = creditCard.ZipCode,
                //    LastTokenValidateDateTime = creditCard.LastTokenValidateDateTime
                //}, _logger);

                //var result = await Client.PostAsync(new CreditCardRequest
                //{
                //    // do we need to set all properties?
                //    // can we update the credit card like this?
                //        CreditCardCompany = creditCard.CreditCardCompany,
                //        CreditCardId = creditCard.CreditCardId,
                //        NameOnCard = creditCard.NameOnCard,
                //        Last4Digits = creditCard.Last4Digits,
                //        ExpirationMonth = creditCard.ExpirationMonth,
                //        ExpirationYear = creditCard.ExpirationYear,
                //        Token = creditCard.Token,
                //        Label = creditCard.Label.ToString(),
                //        ZipCode = creditCard.ZipCode,
                //        LastTokenValidateDateTime = creditCard.LastTokenValidateDateTime                        
                //}, _logger);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

//        public Task UpdateFavoriteAddress(SaveAddress address)
//        {
//            var req = string.Format("/account/addresses/{0}", address.Id);
//            return Client.PutAsync<string>(req, address, logger: Logger);
//        }

        //private void UpdateCreditCard(CreditCardDetails creditCard)
        //{
        // somehow send a
        //var request = new CreditCardRequest
        //{
        //    CreditCardCompany = creditCard.CreditCardCompany,
        //    CreditCardId = creditCard.CreditCardId,
        //    NameOnCard = creditCard.NameOnCard,
        //    Last4Digits = creditCard.Last4Digits,
        //    ExpirationMonth = creditCard.ExpirationMonth,
        //    ExpirationYear = creditCard.ExpirationYear,
        //    Token = creditCard.Token,
        //    Label = creditCard.Label.ToString(),
        //    ZipCode = creditCard.ZipCode,
        //    LastTokenValidateDateTime = creditCard.LastTokenValidateDateTime
        //};
        //}

        //private Task<> AuthenticateAsync(CreditCardAddedOrUpdated auth)
        // {
        //     return Client.PostAsync<AuthResponse>("/account/creditcards/", auth, _logger);
        // }


    }
}