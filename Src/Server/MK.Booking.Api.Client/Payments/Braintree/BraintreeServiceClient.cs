#region

using System;
using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using BraintreeEncryption.Library;

#endregion

namespace apcurium.MK.Booking.Api.Client.Payments.Braintree
{
    public class BraintreeServiceClient : BaseServiceClient, IPaymentServiceClient
    {
        public BraintreeServiceClient(string url, string sessionId, string clientKey, string userAgent)
            : base(url, sessionId, userAgent)
        {
            ClientKey = clientKey;
        }

        protected string ClientKey { get; set; }


        public async Task<TokenizedCreditCardResponse> Tokenize(string creditCardNumber, DateTime expiryDate, string cvv)
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

        public Task<DeleteTokenizedCreditcardResponse> ForgetTokenizedCard(string cardToken)
        {
            return Client.DeleteAsync(new DeleteTokenizedCreditcardBraintreeRequest
            {
                CardToken = cardToken,
            });
        }

        
        public Task<CommitPreauthorizedPaymentResponse> PreAuthorizeAndCommit(string cardToken, double amount,
            double meterAmount, double tipAmount, Guid orderId)
        {
            return Client.PostAsync(new PreAuthorizeAndCommitPaymentBraintreeRequest
            {
                Amount = (decimal) amount,
                MeterAmount = (decimal) meterAmount,
                TipAmount = (decimal) tipAmount,
                CardToken = cardToken,
                OrderId = orderId
            });
        }

        public Task<PairingResponse> Pair(Guid orderId, string cardToken, int? autoTipPercentage, double? autoTipAmount)
        {
            throw new NotImplementedException();
        }

        public Task<BasePaymentResponse> Unpair(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public Task ResendConfirmationToDriver(Guid orderId)
        {
            return Client.PostAsync<string>("/payment/ResendConfirmationRequest", new ResendPaymentConfirmationRequest {OrderId = orderId});
        }
    }
}