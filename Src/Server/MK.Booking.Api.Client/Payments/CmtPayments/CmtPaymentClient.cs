#region

using System;
using System.Globalization;
using System.IO;
using System.Net;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Tokenize;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;

#endregion

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments
{
    /// <summary>
    ///     The Tokenize resource provides developers the ability to create a token in place of
    ///     cardholder data, update tokenized data and delete a token. The token does not use the
    ///     cardholder data to create the token so there is no way to get the cardholder information
    ///     with just the token alone
    /// </summary>
    public class CmtPaymentClient : BaseServiceClient, IPaymentServiceClient
    {
        private readonly ILogger _logger;
        private readonly string _userAgent;

        public CmtPaymentClient(string baseUrl, string sessionId, CmtPaymentSettings cmtSettings, ILogger logger,
            string userAgent)
            : base(baseUrl, sessionId, userAgent)
        {
            _logger = logger;
            _userAgent = userAgent;
            CmtPaymentServiceClient = new CmtPaymentServiceClient(cmtSettings, null, userAgent);
        }

        private CmtPaymentServiceClient CmtPaymentServiceClient { get; set; }

        public TokenizedCreditCardResponse Tokenize(string accountNumber, DateTime expiryDate, string cvv)
        {
            return Tokenize(CmtPaymentServiceClient, accountNumber, expiryDate);
        }

        public DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken)
        {
            return Client.Delete(new DeleteTokenizedCreditcardCmtRequest
            {
                CardToken = cardToken
            });
        }

        public PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, double meterAmount,
            double tipAmount, Guid orderId)
        {
            return Client.Post(new PreAuthorizePaymentCmtRequest
            {
                Amount = amount,
                Meter = meterAmount,
                Tip = tipAmount,
                CardToken = cardToken,
                OrderId = orderId
            });
        }

        public CommitPreauthorizedPaymentResponse CommitPreAuthorized(string transactionId)
        {
            return Client.Post(new CommitPreauthorizedPaymentCmtRequest
            {
                TransactionId = transactionId,
            });
        }

        public CommitPreauthorizedPaymentResponse PreAuthorizeAndCommit(string cardToken, double amount,
            double meterAmount, double tipAmount, Guid orderId)
        {
            return Client.Post(new PreAuthorizeAndCommitPaymentCmtRequest
            {
                Amount = amount,
                MeterAmount = meterAmount,
                TipAmount = tipAmount,
                CardToken = cardToken,
                OrderId = orderId
            });
        }

        public void ResendConfirmationToDriver(Guid orderId)
        {
            Client.Post(new ResendPaymentConfirmationRequest {OrderId = orderId});
        }

        public PairingResponse Pair(Guid orderId, string cardToken, int? autoTipPercentage, double? autoTipAmount)
        {
            return Client.Post(new PairingRidelinqCmtRequest
            {
                OrderId = orderId,
                CardToken = cardToken,
                AutoTipAmount = autoTipAmount,
                AutoTipPercentage = autoTipPercentage
            });
        }

        public BasePaymentResponse Unpair(Guid orderId)
        {
            return Client.Post(new UnpairingRidelinqCmtRequest
            {
                OrderId = orderId
            });
        }

        private static TokenizedCreditCardResponse Tokenize(CmtPaymentServiceClient cmtPaymentServiceClient,
            string accountNumber, DateTime expiryDate)
        {
            try
            {
                var response = cmtPaymentServiceClient.Post(new TokenizeRequest
                {
                    AccountNumber = accountNumber,
                    ExpiryDate = expiryDate.ToString("yyMM", CultureInfo.InvariantCulture)
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

        public static bool TestClient(CmtPaymentSettings serverPaymentSettings, string number, DateTime date)
        {
            var cmtPaymentServiceClient = new CmtPaymentServiceClient(serverPaymentSettings, null, "test");
            return Tokenize(cmtPaymentServiceClient, number, date).IsSuccessfull;
        }
    }
}