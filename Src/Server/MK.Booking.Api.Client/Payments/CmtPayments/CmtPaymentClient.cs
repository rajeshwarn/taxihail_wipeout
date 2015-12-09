using System;
using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Resources;
using CMTPayment;
using CMTPayment.Extensions;
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
        private readonly CmtPaymentSettings _cmtSettings;

        public CmtPaymentClient(string baseUrl, string sessionId, CmtPaymentSettings cmtSettings,
            IPackageInfo packageInfo, ILogger logger)
            : base(baseUrl, sessionId, packageInfo)
        {
            _cmtSettings = cmtSettings;
        }
        
        public Task<TokenizedCreditCardResponse> Tokenize(string accountNumber, DateTime expiryDate, string cvv, string zipCode = null)
        {
            var cmtPaymentServiceClient = new CmtPaymentServiceClient(_cmtSettings, ServiceType.Taxi, null, null, null); // Use default ServiceType as it doesn't matter
            return Tokenize(cmtPaymentServiceClient, accountNumber, expiryDate, cvv, zipCode);
        }

		/// <summary>
		/// This method does not remove CMT token in CMT payment service, according to ticket https://apcurium.atlassian.net/browse/MKTAXI-3225
		/// </summary>
		/// <param name="cardToken"></param>
		/// <returns></returns>
        public async Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken)
        {
			return new DeleteTokenizedCreditcardResponse();
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

        private static async Task<TokenizedCreditCardResponse> Tokenize(CmtPaymentServiceClient cmtPaymentServiceClient,
            string accountNumber, DateTime expiryDate, string cvv, string zipCode = null)
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
                    };
                
                if(!string.IsNullOrEmpty(zipCode))
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

        private static TokenizedCreditCardResponse TokenizeSyncForSettingsTest(CmtPaymentServiceClient cmtPaymentServiceClient, string accountNumber, DateTime expiryDate)
        {
            try
            {
                var response = cmtPaymentServiceClient.PostAsync(new TokenizeRequest
                {
                    AccountNumber = accountNumber,
                    ExpiryDate = expiryDate.ToString("yyMM", CultureInfo.InvariantCulture),
                    ValidateAccountInformation = false //this must be false when testing because we try to tokenize a fake card
                });

                response.Wait();

                return new TokenizedCreditCardResponse
                {
                    CardOnFileToken = response.Result.CardOnFileToken,
                    IsSuccessful = response.Result.ResponseCode == 1,
                    Message = response.Result.ResponseMessage,
                    CardType = response.Result.CardType,
                    LastFour = response.Result.LastFour,
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

        public static bool TestClient(CmtPaymentSettings serverPaymentSettings, string number, DateTime date, ILogger logger, ServiceType serviceType)
        {
            var cmtPaymentServiceClient = new CmtPaymentServiceClient(serverPaymentSettings, serviceType, null, null, logger);
            var result = TokenizeSyncForSettingsTest(cmtPaymentServiceClient, number, date);
            return result.IsSuccessful;
        }
    }
}