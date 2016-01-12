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
using ServiceStack.Common.Web;
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

        public object Post(AddPaymentMethodRequest request)
        {
            var userId = Guid.Parse(this.GetSession().UserAuthId);
            var account = _accountDao.FindById(userId);

            var creditCardId = Guid.NewGuid();

            if (request.PaymentMethod == PaymentMethods.CreditCard)
            {
                var creditCardResult = BraintreeGateway.CreditCard.Create(new CreditCardRequest
                {
                    CustomerId = account.BraintreeAccountId,
                    PaymentMethodNonce = request.Nonce,
                });

                var creditCard = creditCardResult.Target;

                _commandBus.Send(new AddOrUpdateCreditCard
                {
                    AccountId = userId,
                    CreditCardId = creditCardId,
                    BraintreeAccountId = account.BraintreeAccountId,
                    CreditCardCompany = creditCard.CardType.ToString(),
                    NameOnCard = request.CardholderName,
                    Last4Digits = creditCard.LastFour,
                    Token = creditCard.Token,
                    Label = CreditCardLabelConstants.Personal.ToString(),
                    ExpirationMonth = creditCard.ExpirationMonth,
                    ExpirationYear = creditCard.ExpirationYear,
                });

                return new TokenizedCreditCardResponse
                {
                    CardOnFileToken = creditCard.Token,
                    CreditCardId = creditCardId,
                    CardType = creditCard.CardType.ToString(),
                    LastFour = creditCard.LastFour,
                    BraintreeAccountId = creditCard.CustomerId,
                    IsSuccessful = creditCardResult.IsSuccess(),
                    Message = creditCardResult.Message
                };
            }

            var paymentMethodResult = BraintreeGateway.PaymentMethod.Create(new PaymentMethodRequest()
            {
                CustomerId = account.BraintreeAccountId,
                PaymentMethodNonce = request.Nonce,
            });
            
            var paymentMethod = paymentMethodResult.Target;

            if (request.PaymentMethod == PaymentMethods.Paypal)
            {
                var paypalResult = BraintreeGateway.PayPalAccount.Find(paymentMethod.Token);

                _commandBus.Send(new AddOrUpdateCreditCard
                {
                    AccountId = userId,
                    CreditCardId = creditCardId,
                    BraintreeAccountId = account.BraintreeAccountId,
                    CreditCardCompany = "Paypal",
                    NameOnCard = paypalResult.Email,
                    Token = paymentMethod.Token
                });
            }
            else
            {
                _commandBus.Send(new AddOrUpdateCreditCard
                {
                    AccountId = userId,
                    CreditCardId = creditCardId,
                    BraintreeAccountId = account.BraintreeAccountId,
                    CreditCardCompany = request.PaymentMethod.ToString(),
                    NameOnCard = request.CardholderName??account.Name,
                    Token = paymentMethod.Token
                });
            }
            

            return new TokenizedCreditCardResponse
            {
                CardOnFileToken = paymentMethod.Token,
                PaymentMethod = request.PaymentMethod,
                CreditCardId = creditCardId,
                BraintreeAccountId = paymentMethod.CustomerId,
                IsSuccessful = paymentMethodResult.IsSuccess(),
                Message = paymentMethodResult.Message
            };
        }

        public object Get(GenerateClientTokenBraintreeRequest request)
        {
            try
            {
                var userId = Guid.Parse(this.GetSession().UserAuthId);
                var account = _accountDao.FindById(userId);

                if (account.BraintreeAccountId.HasValueTrimmed())
                {
                    return GetClientToken(account.BraintreeAccountId);
                }

                var customerId = GetOrGenerateBraintreeCustomerId(account);

                _commandBus.Send(new AddBraintreeAccountId()
                {
                    AccountId = account.Id,
                    BraintreeAccountId = customerId
                });

                return GetClientToken(account.BraintreeAccountId);
            }
            catch (Exception ex)
            {
                throw new HttpException((int) HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private object GetClientToken(string customerId)
        {
            var tokenRequest = new ClientTokenRequest()
            {
                CustomerId = customerId,
                MerchantAccountId = BraintreeGateway.MerchantId,
                
            };

            
            var clientToken = BraintreeGateway.ClientToken.generate(tokenRequest);

            return new GenerateClientTokenResponse()
            {
                ClientToken = clientToken
            };
        }

        private string GetOrGenerateBraintreeCustomerId(AccountDetail account)
        {
            var name = account.SelectOrDefault(acc => acc.Name.Split(' '), new string[0]);

            if (account.DefaultCreditCard.HasValue && account.DefaultCreditCard != Guid.Empty)
            {
                var creditcard = _creditCardDao.FindById(account.DefaultCreditCard.Value);

                var braintreeCreditCard = BraintreeGateway.CreditCard.Find(creditcard.Token);

                var braintreeCustomerUpdate = new CustomerRequest()
                {
                    FirstName = name.FirstOrDefault(),
                    LastName = name.LastOrDefault()
                };

                BraintreeGateway.Customer.Update(braintreeCreditCard.CustomerId, braintreeCustomerUpdate);

                return braintreeCreditCard.CustomerId;
            }

            

            var braintreeCustomerCreation = new CustomerRequest()
            {
                FirstName = name.FirstOrDefault(),
                LastName = name.LastOrDefault()
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