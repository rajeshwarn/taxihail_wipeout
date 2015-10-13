using System;
using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Resources;
using BraintreeEncryption.Library;

namespace apcurium.MK.Booking.Api.Client.Payments.Braintree
{
    public class BraintreeServiceClient : BaseServiceClient, IPaymentServiceClient
    {
        public BraintreeServiceClient(string url, string sessionId, string clientKey, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
            ClientKey = clientKey;
        }

        protected string ClientKey { get; set; }

        public async Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, DateTime expiryDate, string cvv, string zipCode = null)
        {
            try
            {
                var braintree = new BraintreeEncrypter(ClientKey);
                var encryptedNumber = braintree.Encrypt(creditCardNumber);
                var encryptedExpirationDate = braintree.Encrypt(expiryDate.ToString("MM/yyyy", CultureInfo.InvariantCulture));
                var encryptedCvv = braintree.Encrypt(cvv);

                var result = await Client.PostAsync(new TokenizeCreditCardBraintreeRequest
                {
                    EncryptedCreditCardNumber = encryptedNumber,
                    EncryptedExpirationDate = encryptedExpirationDate,
                    EncryptedCvv = encryptedCvv,
                });
                return result;
            }
            catch (Exception e)
            {
                var message = e.Message;
                var exception = e as AggregateException;
                if (exception != null)
                {
                    message = exception.InnerException.Message;
                }

                return new TokenizedCreditCardResponse
                {
                    IsSuccessful = false,
                    Message = message
                };
            }
        }

        public Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken)
        {
            return Client.DeleteAsync(new DeleteTokenizedCreditcardRequest
            {
                CardToken = cardToken
            });
        }

        public Task<OverduePayment> GetOverduePayment()
        {
            return Client.GetAsync<OverduePayment>("/account/overduepayment");
        }

        public Task<SettleOverduePaymentResponse> SettleOverduePayment()
        {
            return Client.PostAsync(new SettleOverduePaymentRequest());
        }
    }
}