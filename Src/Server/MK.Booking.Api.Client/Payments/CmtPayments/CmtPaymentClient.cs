﻿using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
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
        public CmtPaymentClient(string baseUrl, string sessionId, CmtPaymentSettings cmtSettings,
            IPackageInfo packageInfo, ILogger logger)
            : base(baseUrl, sessionId, packageInfo)
        {
            CmtPaymentServiceClient = new CmtPaymentServiceClient(cmtSettings, null, packageInfo, logger);
        }

        private CmtPaymentServiceClient CmtPaymentServiceClient { get; set; }

        public Task<TokenizedCreditCardResponse> Tokenize(string accountNumber, DateTime expiryDate, string cvv)
        {
            return Tokenize(CmtPaymentServiceClient, accountNumber, expiryDate);
        }

        public async Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken)
        {
            var result = await Client.DeleteAsync(new DeleteTokenizedCreditcardRequest
            {
                CardToken = cardToken
            });
            return result;
        }

        public Task ResendConfirmationToDriver(Guid orderId)
        {
            return Client.PostAsync<string>("/payment/ResendConfirmationRequest", new ResendPaymentConfirmationRequest { OrderId = orderId });
        }

        public async Task<PairingResponse> Pair(Guid orderId, string cardToken, int? autoTipPercentage)
        {
            try
            {
                var response = await Client.PostAsync(new PairingForPaymentRequest
                {
                    OrderId = orderId,
                    CardToken = cardToken,
                    AutoTipPercentage = autoTipPercentage

                });
                return response;
            }
            catch (ServiceStack.ServiceClient.Web.WebServiceException)
            {                
                return new PairingResponse { IsSuccessful = false };
            }            
        }

        public Task<BasePaymentResponse> Unpair(Guid orderId)
        {
            return Client.PostAsync(new UnpairingForPaymentRequest
            {
                OrderId = orderId
            });
        }

        private static async Task<TokenizedCreditCardResponse> Tokenize(CmtPaymentServiceClient cmtPaymentServiceClient,
            string accountNumber, DateTime expiryDate)
        {
            try
            {
                var response = await cmtPaymentServiceClient.PostAsync(new TokenizeRequest
                {
                    AccountNumber = accountNumber,
                    ExpiryDate = expiryDate.ToString("yyMM", CultureInfo.InvariantCulture)
#if DEBUG
                    ,ValidateAccountInformation = false
#endif
                });

                return new TokenizedCreditCardResponse
                {
                    CardOnFileToken = response.CardOnFileToken,
                    IsSuccessful = response.ResponseCode == 1,
                    Message = response.ResponseMessage,
                    CardType = response.CardType,
                    LastFour = response.LastFour,
                };
            }
            catch (WebException e)
            {
                return new TokenizedCreditCardResponse
                {
                    IsSuccessful = false,
                    Message = e.Message
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
            catch (WebException e)
            {
                return new TokenizedCreditCardResponse
                {
                    IsSuccessful = false,
                    Message = e.Message
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