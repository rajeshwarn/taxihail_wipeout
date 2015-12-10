using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests.Payment.Braintree;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using Braintree;
using BraintreeEncryption.Library;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;
using Environment = Braintree.Environment;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class BraintreeClientPaymentService : Service
    {
        private readonly IAccountDao _accountDao;
        private BraintreeGateway BraintreeGateway { get; set; }
        private readonly ICommandBus _commandBus;
        private readonly ICreditCardDao _creditCardDao;

        public BraintreeClientPaymentService(IServerSettings serverSettings, IAccountDao accountDao, ICommandBus commandBus, ICreditCardDao creditCardDao)
        {
            _accountDao = accountDao;
            _commandBus = commandBus;
            _creditCardDao = creditCardDao;
            BraintreeGateway = GetBraintreeGateway(serverSettings.GetPaymentSettings().BraintreeServerSettings);
        }

        public TokenizedCreditCardResponse Post(TokenizeCreditCardBraintreeRequest tokenizeRequest)
        {
            var userId = Guid.Parse(this.GetSession().UserAuthId);
            var account = _accountDao.FindById(userId);

            return TokenizedCreditCard(BraintreeGateway,
                account,
                tokenizeRequest.EncryptedCreditCardNumber,
                tokenizeRequest.EncryptedExpirationDate,
                tokenizeRequest.EncryptedCvv,
                tokenizeRequest.PaymentMethodNonce);
        }

        public object Get(GenerateClientTokenBraintreeRequest request)
        {
            try
            {
                var userId = Guid.Parse(this.GetSession().UserAuthId);
                var account = _accountDao.FindById(userId);

                if (account.BraintreeAccountId.HasValueTrimmed())
                {
                    var tokenRequest = new ClientTokenRequest()
                    {
                        CustomerId = account.BraintreeAccountId,
                        MerchantAccountId = BraintreeGateway.MerchantId,
                    };

                    return BraintreeGateway.ClientToken.generate(tokenRequest);
                }

                var customerId = GetOrGenerateBraintreeCustomerId(account);

                _commandBus.Send(new AddBraintreeAccountId()
                {
                    AccountId = account.Id,
                    BraintreeAccountId = customerId
                });

                return BraintreeGateway.ClientToken.generate(new ClientTokenRequest()
                {
                    CustomerId = customerId
                });

            }
            catch (Exception ex)
            {
                throw new HttpException((int) HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private string GetOrGenerateBraintreeCustomerId(AccountDetail account)
        {
            var name = account.SelectOrDefault(acc => acc.Name.Split(' '), new string[0]);

            if (account.DefaultCreditCard.HasValue)
            {
                var creditcard = _creditCardDao.FindById(account.DefaultCreditCard.Value);

                var braintreeCreditCard = BraintreeGateway.CreditCard.Find(creditcard.Token);

                var braintreeCustomerUpdate = new CustomerRequest()
                {
                    FirstName = name.FirstOrDefault(),
                    LastName = name.FirstOrDefault()
                };

                BraintreeGateway.Customer.Update(braintreeCreditCard.CustomerId, braintreeCustomerUpdate);

                return braintreeCreditCard.CustomerId;
            }

            var braintreeCustomerCreation = new CustomerRequest()
            {
                FirstName = name.FirstOrDefault(),
                LastName = name.FirstOrDefault()
            };

            var result = BraintreeGateway.Customer.Create(braintreeCustomerCreation);

            var customer = result.Target;
            return customer.Id;
        }

        public static bool TestClient(BraintreeServerSettings settings, BraintreeClientSettings braintreeClientSettings)
        {
            var client = GetBraintreeGateway(settings);

            var dummyCreditCard = new TestCreditCards(TestCreditCards.TestCreditCardSetting.Braintree).Visa;

            var braintreeEncrypter = new BraintreeEncrypter(braintreeClientSettings.ClientKey);

            return TokenizedCreditCard(client,null, braintreeEncrypter.Encrypt(dummyCreditCard.Number),
                    braintreeEncrypter.Encrypt(dummyCreditCard.ExpirationDate.ToString("MM/yyyy", CultureInfo.InvariantCulture)),
                    braintreeEncrypter.Encrypt(dummyCreditCard.AvcCvvCvv2 + "")
                ).IsSuccessful;
        }
        
        //TODO MKTAXI-3005: Validate if we still need this after vZero is implemented.
        private static TokenizedCreditCardResponse TokenizedCreditCard(
            BraintreeGateway client,
            AccountDetail account,
            string encryptedCreditCardNumber,
            string encryptedExpirationDate,
            string encryptedCvv,
            string paymentMethodNonce = null)
        {
            try
            {
                var name = account.SelectOrDefault(acc => acc.Name.Split(' '), new string[0]);

                var request = new CustomerRequest
                {
                    FirstName = name.FirstOrDefault(),
                    LastName = name.LastOrDefault(),
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

                var hasBraintreeAccountId = account.SelectOrDefault(acc => acc.BraintreeAccountId.HasValueTrimmed(), false);
                
                var result = hasBraintreeAccountId
                    ? client.Customer.Update(account.BraintreeAccountId, request)
                    : client.Customer.Create(request);

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

                var creditCard = customer.CreditCards.OrderByDescending(card => card.CreatedAt).First();

                return new TokenizedCreditCardResponse
                {
                    CardOnFileToken = creditCard.Token,
                    CardType = creditCard.CardType.ToString(),
                    LastFour = creditCard.LastFour,
                    BraintreeAccountId = customer.Id,
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
                return customer.CreditCards.OrderByDescending(card => card.CreatedAt)
                    .Select(card => card.Verification.CvvResponseCode != "N")
                    .FirstOrDefault();
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