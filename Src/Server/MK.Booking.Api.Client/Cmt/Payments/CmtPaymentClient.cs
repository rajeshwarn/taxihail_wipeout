using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;
using System;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Capture;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;
using apcurium.MK.Common.Configuration;
using MK.Booking.Api.Client.Android;


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

		public AuthorizationResponse PreAuthorizeTransaction(string cardToken, double amount)
		{
			return Client.Post(new AuthorizationRequest()
			{
				Amount = (int)(amount*100),
				CardOnFileToken = cardToken,
				CurrencyCode = _appSettings.GetSetting(AuthorizationRequest.CurrencyCodes.CurrencyCodeString),
				TransactionType = AuthorizationRequest.TransactionTypes.PreAuthorized,
				CardReaderMethod = AuthorizationRequest.CardReaderMethods.Manual
			});
		}
		
		public CaptureResponse CapturePreAuthorized(long transactionId)
        {
            return Client.Post(new CaptureRequest
                {
                TransactionId = transactionId
            });
        }


		public long PreAuthorize (string cardToken, double amount)
		{
			var response = PreAuthorizeTransaction(cardToken,amount);
			if(response.ResponseCode == 0)
			{
				return response.TransactionId;
			}
			return -1;
		}
		public bool CommitPreAuthorized (long transactionId)
		{
			return CapturePreAuthorized(transactionId).ResponseCode == 0;
		}
    }
}
