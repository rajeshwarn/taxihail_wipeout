using System;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration.Impl;


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
		public CmtPaymentClient(string baseUrl,string sessionId, CmtPaymentSettings cmtSettings)
            : base(baseUrl,sessionId)
        {
			CmtClient = new CmtPaymentServiceClient(cmtSettings,true);
        }

        private CmtPaymentServiceClient CmtClient { get; set; }

        public TokenizedCreditCardResponse Tokenize(string accountNumber, DateTime expiryDate, string cvv)
        {
            var response = CmtClient.Post(new TokenizeRequest
            {
                AccountNumber = accountNumber,
                ExpiryDateYYMM = expiryDate.ToString("yyMM")
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

        public DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken)
        {
            return Client.Post(new DeleteTokenizedCreditcardCmtRequest()
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
                    TransactionId = transactionId
                });
        }

        



    }
}
