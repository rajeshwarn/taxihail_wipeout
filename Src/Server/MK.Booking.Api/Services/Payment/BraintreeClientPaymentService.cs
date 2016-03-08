using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using Braintree;
using BraintreeEncryption.Library;
using Environment = Braintree.Environment;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class BraintreeClientPaymentService : BaseApiService
    {
        private BraintreeGateway BraintreeGateway { get; set; }

        public BraintreeClientPaymentService(IServerSettings serverSettings)
        {
            BraintreeGateway = GetBraintreeGateway(serverSettings.GetPaymentSettings().BraintreeServerSettings);
        }

        public TokenizedCreditCardResponse Post(TokenizeCreditCardBraintreeRequest tokenizeRequest)
        {
            return TokenizedCreditCard(BraintreeGateway,
                tokenizeRequest.EncryptedCreditCardNumber,
                tokenizeRequest.EncryptedExpirationDate,
                tokenizeRequest.EncryptedCvv,
                tokenizeRequest.PaymentMethodNonce);
        }

        public string Get()
        {
            try
            {
                return BraintreeGateway.ClientToken.generate();
            }
            catch (Exception ex)
            {
                throw new HttpException((int) HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public static bool TestClient(BraintreeServerSettings settings, BraintreeClientSettings braintreeClientSettings)
        {
            var client = GetBraintreeGateway(settings);

            var dummyCreditCard = new TestCreditCards(TestCreditCards.TestCreditCardSetting.Braintree).Visa;

            var braintreeEncrypter = new BraintreeEncrypter(braintreeClientSettings.ClientKey);

            return TokenizedCreditCard(client, braintreeEncrypter.Encrypt(dummyCreditCard.Number),
                    braintreeEncrypter.Encrypt(dummyCreditCard.ExpirationDate.ToString("MM/yyyy", CultureInfo.InvariantCulture)),
                    braintreeEncrypter.Encrypt(dummyCreditCard.AvcCvvCvv2 + "")
                ).IsSuccessful;
        }

        private static TokenizedCreditCardResponse TokenizedCreditCard(
            BraintreeGateway client,
            string encryptedCreditCardNumber,
            string encryptedExpirationDate,
            string encryptedCvv,
            string paymentMethodNonce = null)
        {
            try
            {
                var request = new CustomerRequest
                {
                    CreditCard = new CreditCardRequest
                    {
                        Number = encryptedCreditCardNumber,
                        ExpirationDate = encryptedExpirationDate,
                        CVV = encryptedCvv,
                        PaymentMethodNonce = paymentMethodNonce, // Used for tokenization from javascript API
                        Options = new CreditCardOptionsRequest
                        {
                            VerifyCard = true
                        }
                    }
                };

                var result = client.Customer.Create(request);
                var customer = result.Target;

                var creditCardCvvSuccess = CheckCvvResponseCodeForSuccess(customer);
                

                if (!result.IsSuccess() || !creditCardCvvSuccess)
                {
                    return new TokenizedCreditCardResponse
                    {
                        IsSuccessful = false,
                        Message = result.Message
                    };
                }

                var creditCard = customer.CreditCards.First();

                return new TokenizedCreditCardResponse
                {
                    CardOnFileToken = creditCard.Token,
                    CardType = creditCard.CardType.ToString(),
                    LastFour = creditCard.LastFour,
                    IsSuccessful = result.IsSuccess(),
                    Message = result.Message
                };
            }
            catch (Exception e)
            {
                return new TokenizedCreditCardResponse
                {
                    IsSuccessful = false,
                    Message = e.Message
                };
            }
        }

        private static bool CheckCvvResponseCodeForSuccess(Customer customer)
        {
            try
            {
                // "M" = matches, "N" = does not match, "U" = not verified, "S" = bank doesn't participate, "I" = not provided
                return customer.CreditCards[0].Verification.CvvResponseCode != "N";
            }
            catch (Exception)
            {
                // an error occured
                return false;
            }
        }

        private static BraintreeGateway GetBraintreeGateway(BraintreeServerSettings settings)
        {
            var env = Environment.SANDBOX;
            if (!settings.IsSandbox)
            {
                env = Environment.PRODUCTION;
            }

            return new BraintreeGateway
            {
                Environment = env,
                MerchantId = settings.MerchantId,
                PublicKey = settings.PublicKey,
                PrivateKey = settings.PrivateKey,
            };
        }
    }
}