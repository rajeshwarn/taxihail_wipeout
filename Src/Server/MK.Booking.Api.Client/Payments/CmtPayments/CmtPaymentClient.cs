using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;
using System;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Capture;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration;
using MK.Booking.Api.Client;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;


namespace apcurium.MK.Booking.Api.Client.Cmt.Payments
{
    /// <summary>
    /// The Tokenize resource provides developers the ability to create a token in place of 
    /// cardholder data, update tokenized data and delete a token. The token does not use the 
    /// cardholder data to create the token so there is no way to get the cardholder information 
    /// with just the token alone
    /// </summary>
	public class CmtPaymentClient : CmtBasePaymentServiceClient, IPaymentServiceClient
    {
		private readonly string _currencyCode;



        public CmtPaymentClient(CmtPaymentSettings settings)
            : base(settings.BaseUrl,settings.CustomerKey,settings.ConsumerSecretKey,true)
        {
            _currencyCode = settings.CurrencyCode;
        }

        public TokenizedCreditCardResponse Tokenize(string accountNumber, DateTime expiryDate, string cvv)
        {
            var response = Client.Post(new TokenizeRequest
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
            var response = Client.Delete(new TokenizeDeleteRequest()
                {
                    CardToken = cardToken
                });

            return new DeleteTokenizedCreditcardResponse()
                {
                    IsSuccessfull = response.ResponseCode == 1,
                    Message = response.ResponseMessage
                };
        }


        public CommitPreauthoriedPaymentResponse CommitPreAuthorized(string transactionId, string orderNumber)
        {
            var response = Client.Post(new CaptureRequest
            {
                TransactionId = transactionId.ToLong(),
                L3Data = new LevelThreeData()
                {
                    PurchaseOrderNumber = orderNumber
                }
            });

            return new CommitPreauthoriedPaymentResponse()
            {
                IsSuccessfull = response.ResponseCode == 1,
                Message = response.ResponseMessage,
            };
        }

        public PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, string orderNumber)
		{
            var request = new AuthorizationRequest()
            {
                Amount = (int)(amount * 100),
                CardOnFileToken = cardToken,
                CurrencyCode = _currencyCode,
                TransactionType = AuthorizationRequest.TransactionTypes.PreAuthorized,
                CardReaderMethod = AuthorizationRequest.CardReaderMethods.Manual,
                L3Data = new LevelThreeData()
                {
                    PurchaseOrderNumber = orderNumber
                }
            };
            var response = Client.Post(request);

            return new PreAuthorizePaymentResponse()
            {
                IsSuccessfull = response.ResponseCode == 1,
                Message = response.ResponseMessage,
                TransactionId = response.TransactionId + "",
            };
		}



    }
}
