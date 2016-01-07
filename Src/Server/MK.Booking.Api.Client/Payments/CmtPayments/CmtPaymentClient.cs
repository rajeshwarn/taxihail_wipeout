using System;
using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;

#if CLIENT
using MK.Common.Exceptions;
#else
using apcurium.MK.Booking.Api.Client.Extensions;
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

        public CmtPaymentClient(string baseUrl, string sessionId, CmtPaymentSettings cmtSettings, IIPAddressManager ipAddressManager, IPackageInfo packageInfo, ILogger logger)
            : base(baseUrl, sessionId, packageInfo)
        {
            _ipAddressManager = ipAddressManager;

            CmtPaymentServiceClient = new CmtPaymentServiceClient(cmtSettings, null, packageInfo, logger);
        }

        private CmtPaymentServiceClient CmtPaymentServiceClient { get; set; }

        public Task<TokenizedCreditCardResponse> Tokenize(string accountNumber, DateTime expiryDate, string cvv, string kountSessionId, string zipCode, Account account)
        {
            return Tokenize(CmtPaymentServiceClient, accountNumber, expiryDate, cvv, kountSessionId, zipCode, account);
        }

        public async Task<BasePaymentResponse> ValidateTokenizedCard(string cardToken, string cvv, string kountSessionId, string zipCode, Account account)
        {
            try
            {
                var request = new TokenizeValidateRequest
                {
                    Token = cardToken,
                    Cvv = cvv,
                    SessionId = kountSessionId,
                    Email = account.Email,
                    CustomerIpAddress = _ipAddressManager.GetIPAddress()
                };

                if(zipCode.HasValue())
                {
                    request.ZipCode = zipCode;
                }

                var response = await CmtPaymentServiceClient.PostAsync(request);

                return new BasePaymentResponse
                {
                    IsSuccessful = response.ResponseCode == 1,
                    Message = response.ResponseMessage
                };
            }
            catch(Exception e)
            {
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
            return Client.GetAsync<OverduePayment>("/account/overduepayment");
        }

        public Task<SettleOverduePaymentResponse> SettleOverduePayment()
        {
            return Client.PostAsync(new SettleOverduePaymentRequest());
        }

        public Task<BasePaymentResponse> Unpair(Guid orderId)
        {
            return Client.PostAsync(new UnpairingForPaymentRequest
            {
                OrderId = orderId
            });
        }

        private async Task<TokenizedCreditCardResponse> Tokenize(CmtPaymentServiceClient cmtPaymentServiceClient,
            string accountNumber, DateTime expiryDate, string cvv, string kountSessionId, string zipCode, Account account)
        {
            try
            {
                var request = new TokenizeRequest
                {
                    AccountNumber = accountNumber,
                    ExpiryDate = expiryDate.ToString("yyMM", CultureInfo.InvariantCulture),
                    #if DEBUG
                    ValidateAccountInformation = false,
                    #endif
                    Cvv = cvv,
                    SessionId = kountSessionId,
                    Email = account.Email,
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
            var cmtPaymentServiceClient = new CmtPaymentServiceClient(serverPaymentSettings, null, null, logger);
            var result = TokenizeSyncForSettingsTest(cmtPaymentServiceClient, number, date);
            return result.IsSuccessful;
        }
    }
}