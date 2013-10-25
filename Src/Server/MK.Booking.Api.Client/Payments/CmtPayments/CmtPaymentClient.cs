using System;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Cmt;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using System.Net;
using System.IO;
using apcurium.MK.Common.Diagnostic;


namespace apcurium.MK.Booking.Api.Client.Cmt.Payments
{
    /// <summary>
    /// The Tokenize resource provides developers the ability to create a token in place of 
    /// cardholder data, update tokenized data and delete a token. The token does not use the 
    /// cardholder data to create the token so there is no way to get the cardholder information 
    /// with just the token alone
    /// </summary>
    public class CmtPaymentClient : BaseServiceClient, IPaymentServiceClient
    {
        readonly ILogger _logger;
        readonly string _userAgent;
		public CmtPaymentClient(string baseUrl,string sessionId, CmtPaymentSettings cmtSettings, ILogger logger, string userAgent)
            : base(baseUrl,sessionId, userAgent)
        {
            _logger = logger;
            _userAgent = userAgent;
            CmtClient = new CmtPaymentServiceClient(cmtSettings,null,userAgent);

        }

        private CmtPaymentServiceClient CmtClient { get; set; }

        public TokenizedCreditCardResponse Tokenize(string accountNumber, DateTime expiryDate, string cvv)
        {
            return Tokenize(CmtClient, accountNumber, expiryDate);
        }

        private static TokenizedCreditCardResponse Tokenize(CmtPaymentServiceClient cmtClient, string accountNumber, DateTime expiryDate)
        {
            try
            {
                
            var response = cmtClient.Post(new TokenizeRequest
                {
                    AccountNumber = accountNumber,
                    ExpiryDate = expiryDate.ToString("yyMM")
                });

            return new TokenizedCreditCardResponse()
                {
                    CardOnFileToken = response.CardOnFileToken,
                    IsSuccessfull = response.ResponseCode == 1,
                    Message = response.ResponseMessage,
                    CardType = response.CardType,
                    LastFour = response.LastFour,
                };
            }
            catch(WebException e)
            {
                var x= new StreamReader(e.Response.GetResponseStream()).ReadToEnd();

                return new TokenizedCreditCardResponse()
                {
                    IsSuccessfull = false,
                    Message = e.Message
                };
            }
        }

        public DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken)
        {
            return Client.Delete(new DeleteTokenizedCreditcardCmtRequest()
                {
                    CardToken = cardToken
                });
        }

        public PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, Guid orderId)
        {
            return Client.Post(new PreAuthorizePaymentCmtRequest()
                {
                    Amount = amount,
                    CardToken = cardToken,
                    OrderId = orderId
                });

        }


        public CommitPreauthorizedPaymentResponse CommitPreAuthorized(string transactionId)
        {
            return Client.Post(new CommitPreauthorizedPaymentCmtRequest()
                {
                    TransactionId = transactionId,
                });
        }

        public void ResendConfirmationToDriver(Guid orderId)
        {
            Client.Post(new ResendPaymentConfirmationRequest { OrderId = orderId });
        }

        public static bool TestClient(CmtPaymentSettings serverPaymentSettings, string number, DateTime date)
        {
            var cmtClient =  new CmtPaymentServiceClient(serverPaymentSettings,null, "test");
            return Tokenize(cmtClient, number, date).IsSuccessfull;
        }
    }
}
