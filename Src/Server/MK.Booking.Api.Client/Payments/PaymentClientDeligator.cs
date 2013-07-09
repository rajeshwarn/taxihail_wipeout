using System;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Client.Payments.Braintree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Client.Payments
{
    public class PaymentClientDeligate : IPaymentServiceClient
    {
		private readonly PaymentMethod _paymentMethod;
        private readonly BraintreeServiceClient _braintreeServiceClient;
		private readonly CmtPaymentClient _cmtServiceClient;

		public PaymentClientDeligate(PaymentMethod paymentMethod, BraintreeServiceClient braintreeServiceClient, CmtPaymentClient cmtServiceClient)
        {
			_paymentMethod = paymentMethod;
            _braintreeServiceClient = braintreeServiceClient;
            _cmtServiceClient = cmtServiceClient;
        }

        public IPaymentServiceClient GetClient()
        {
            const string onErrorMessage = "Payment Method not found or unknown";
  

			switch (_paymentMethod)
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