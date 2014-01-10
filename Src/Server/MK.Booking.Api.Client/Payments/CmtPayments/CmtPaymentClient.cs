#region

using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Tokenize;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration.Impl;

#endregion

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
            string userAgent)
            : base(baseUrl, sessionId, userAgent)
        {           
            CmtPaymentServiceClient = new CmtPaymentServiceClient(cmtSettings, null, userAgent);
        }

        private CmtPaymentServiceClient CmtPaymentServiceClient { get; set; }

        public Task<TokenizedCreditCardResponse> Tokenize(string accountNumber, DateTime expiryDate, string cvv)
        {
            return Tokenize(CmtPaymentServiceClient, accountNumber, expiryDate);
        }

        public Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken)
        {
            return Client.DeleteAsync(new DeleteTokenizedCreditcardCmtRequest
            {
                CardToken = cardToken
            });
        }

        public Task<PreAuthorizePaymentResponse> PreAuthorize(string cardToken, double amount, double meterAmount,
            double tipAmount, Guid orderId)
        {
            return Client.PostAsync(new PreAuthorizePaymentCmtRequest
            {
                Amount = amount,
                Meter = meterAmount,
                Tip = tipAmount,
                CardToken = cardToken,
                OrderId = orderId
            });
        }

        public Task<CommitPreauthorizedPaymentResponse> CommitPreAuthorized(string transactionId)
        {
            return Client.PostAsync(new CommitPreauthorizedPaymentCmtRequest
            {
                TransactionId = transactionId,
            });
        }

        public Task<CommitPreauthorizedPaymentResponse> PreAuthorizeAndCommit(string cardToken, double amount,
            double meterAmount, double tipAmount, Guid orderId)
        {
            return Client.PostAsync(new PreAuthorizeAndCommitPaymentCmtRequest
            {
                Amount = amount,
                MeterAmount = meterAmount,
                TipAmount = tipAmount,
                CardToken = cardToken,
                OrderId = orderId
            });
        }

        public Task ResendConfirmationToDriver(Guid orderId)
        {
            return Client.PostAsync<string>("/payment/ResendConfirmationRequest", new ResendPaymentConfirmationRequest { OrderId = orderId });
        }

        public async Task<PairingResponse> Pair(Guid orderId, string cardToken, int? autoTipPercentage, double? autoTipAmount)
        {
            try
            {
                var response = await Client.PostAsync(new PairingRidelinqCmtRequest
                {
                    OrderId = orderId,
                    CardToken = cardToken,
                    AutoTipAmount = autoTipAmount,
                    AutoTipPercentage = autoTipPercentage

                });
                return response;
            }
            catch (ServiceStack.ServiceClient.Web.WebServiceException)
            {                
                return new PairingResponse { IsSuccessfull = false };
            }            
        }

        public Task<BasePaymentResponse> Unpair(Guid orderId)
        {
            return Client.PostAsync(new UnpairingRidelinqCmtRequest
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
                    IsSuccessfull = response.ResponseCode == 1,
                    Message = response.ResponseMessage,
                    CardType = response.CardType,
                    LastFour = response.LastFour,
                };
            }
            catch (WebException e)
            {
                return new TokenizedCreditCardResponse
                {
                    IsSuccessfull = false,
                    Message = e.Message
                };
            }
        }

        public async static Task<bool> TestClient(CmtPaymentSettings serverPaymentSettings, string number, DateTime date)
        {
            var cmtPaymentServiceClient = new CmtPaymentServiceClient(serverPaymentSettings, null, "test");
            var result = await Tokenize(cmtPaymentServiceClient, number, date);
            return result.IsSuccessfull;
        }
    }
}