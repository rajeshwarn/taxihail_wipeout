using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;
using System;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Capture;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;
using apcurium.MK.Common.Configuration;
using MK.Booking.Api.Client;
using apcurium.MK.Common.Extensions;


namespace apcurium.MK.Booking.Api.Client.Cmt.Payments
{
    /// <summary>
    /// The Tokenize resource provides developers the ability to create a token in place of 
    /// cardholder data, update tokenized data and delete a token. The token does not use the 
    /// cardholder data to create the token so there is no way to get the cardholder information 
    /// with just the token alone
    /// </summary>
    public class CmtPaymentClient : CmtPaymentServiceClient, IPaymentClient
    {
        private readonly IConfigurationManager _appSettings;

        public CmtPaymentClient(IConfigurationManager appSettings, bool acceptAllHttps=false)
			: base(acceptAllHttps)
        {
            _appSettings = appSettings;
        }

        public TokenizeResponse Tokenize(string accountNumber, DateTime expiryDate)
        {
            return Client.Post(new TokenizeRequest()
            {
                AccountNumber = accountNumber,
                ExpiryDateYYMM = expiryDate.ToString("yyMM")
            });
        }

        public TokenizeDeleteResponse ForgetTokenizedCard(string cardToken)
        {
            return Client.Delete(new TokenizeDeleteRequest()
                {
                    CardToken = cardToken
                });
        }

		public AuthorizationResponse PreAuthorizeTransaction(string cardToken, double amount, string orderNumber)
		{
			var request = new AuthorizationRequest()
			{
				Amount = (int)(amount*100),
				CardOnFileToken = cardToken,
                CurrencyCode = _appSettings.GetSetting(AuthorizationRequest.CurrencyCodes.CurrencyCodeString).NullIfEmpty()??AuthorizationRequest.CurrencyCodes.Main.UnitedStatesDollar,
				TransactionType = AuthorizationRequest.TransactionTypes.PreAuthorized,
				CardReaderMethod = AuthorizationRequest.CardReaderMethods.Manual,
                L3Data = new LevelThreeData()
                    {
                        PurchaseOrderNumber = orderNumber
                    }
			};
            return Client.Post(request);
		}

        public CaptureResponse CapturePreAuthorized(long transactionId, string orderNumber)
        {
            return Client.Post(new CaptureRequest
            {
                TransactionId = transactionId,
                L3Data = new LevelThreeData()
                {
                    PurchaseOrderNumber = orderNumber
                }
            });
        }


        public long PreAuthorize(string cardToken, double amount, string orderNumber)
		{
			var response = PreAuthorizeTransaction(cardToken,amount,orderNumber);
            if(response.ResponseCode == 1)
			{
				return response.TransactionId;
			}
			return -1;
		}
        public bool CommitPreAuthorized(long transactionId, string orderNumber)
		{
            return CapturePreAuthorized(transactionId, orderNumber).ResponseCode == 1;
		}
    }
}
