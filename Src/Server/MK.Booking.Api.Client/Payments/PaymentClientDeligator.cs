using System;
using MK.Common.Android.Configuration.Impl;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Client.Payments.Braintree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Client.Payments
{
    public class PaymentClientDeligate : IPaymentServiceClient
    {
		private readonly ClientPaymentSettings _clientPaymentSettings;
        private readonly BraintreeServiceClient _braintreeServiceClient;
        private readonly CmtPaymentClient _cmtServiceClient;

		public PaymentClientDeligate(ClientPaymentSettings clientPaymentSettings, BraintreeServiceClient braintreeServiceClient, CmtPaymentClient cmtServiceClient)
        {
			_clientPaymentSettings = clientPaymentSettings;
            _braintreeServiceClient = braintreeServiceClient;
            _cmtServiceClient = cmtServiceClient;
        }

        public IPaymentServiceClient GetClient()
        {
            const string onErrorMessage = "Payment Method not found or unknown";
  

			switch (_clientPaymentSettings.PaymentMode)
            {
                case PaymentMethod.Braintree:
                    return _braintreeServiceClient;
                    
                case PaymentMethod.Cmt:
                    return _cmtServiceClient;

                case PaymentMethod.Fake:
                    return new FakePaymentClient();
                default:
                    throw new Exception(onErrorMessage);
            }
        }
        
        public TokenizedCreditCardResponse Tokenize(string creditCardNumber, DateTime expiryDate, string cvv)
        {
            return GetClient().Tokenize(creditCardNumber, expiryDate, cvv);
        }

        public DeleteTokenizedCreditcardResponse ForgetTokenizedCard(string cardToken)
        {
            return GetClient().ForgetTokenizedCard(cardToken);
        }

        public PreAuthorizePaymentResponse PreAuthorize(string cardToken, double amount, string orderNumber)
        {
            return GetClient().PreAuthorize(cardToken, amount, orderNumber);
        }

        public CommitPreauthorizedPaymentResponse CommitPreAuthorized(string transactionId, string orderNumber)
        {
            return GetClient().CommitPreAuthorized(transactionId, orderNumber);
        }
    }
}