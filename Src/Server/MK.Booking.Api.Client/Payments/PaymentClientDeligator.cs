using System;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.BrainTree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Client.Payments
{
    public class PaymentClientDeligate : IPaymentServiceClient
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly BraintreeServiceClient _braintreeServiceClient;

        public PaymentClientDeligate(IConfigurationManager configurationManager, BraintreeServiceClient braintreeServiceClient)
        {
            _configurationManager = configurationManager;
            _braintreeServiceClient = braintreeServiceClient;
        }

        public IPaymentServiceClient GetClient()
        {
            var paymentSettings = _configurationManager.GetPaymentSettings();


            const string onErrorMessage = "Payment Method not found or unknown";
            if (paymentSettings == null)
            {
                throw new Exception(onErrorMessage);
            }


            switch (paymentSettings.PaymentMode)
            {
                case PaymentSetting.PaymentMethod.Braintree:
                    return _braintreeServiceClient;
                    
                case PaymentSetting.PaymentMethod.Cmt:
                    return new CmtPaymentClient(paymentSettings.CmtPaymentSettings);

                case PaymentSetting.PaymentMethod.Fake:
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

        public CommitPreauthoriedPaymentResponse CommitPreAuthorized(string transactionId, string orderNumber)
        {
            return GetClient().CommitPreAuthorized(transactionId, orderNumber);
        }
    }
}