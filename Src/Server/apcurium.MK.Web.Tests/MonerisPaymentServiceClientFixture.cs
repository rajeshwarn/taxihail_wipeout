using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.Moneris;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Resources;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Module = apcurium.MK.Booking.Module;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class MonerisPaymentServiceClientFixture : BasePaymentClientFixture
    {
        public MonerisPaymentServiceClientFixture() : base(TestCreditCards.TestCreditCardSetting.Moneris)
        {
        }
        public override void Setup()
        {
            base.Setup();
            var paymentService = GetPaymentService();
            UnityServiceLocator.Instance.RegisterInstance<IPaymentService>(paymentService);
        }

        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new MonerisServiceClient(BaseUrl, SessionId, new MonerisPaymentSettings(), new DummyPackageInfo(), null, new Logger());
        }

        protected override PaymentProvider GetProvider()
        {
            return PaymentProvider.Moneris;
        }

        private IPaymentService GetPaymentService()
        {
            var commandBus = UnityServiceLocator.Instance.Resolve<ICommandBus>();
            var logger = UnityServiceLocator.Instance.Resolve<ILogger>();
            var orderPaymentDao = UnityServiceLocator.Instance.Resolve<IOrderPaymentDao>();
            var serverSettings = UnityServiceLocator.Instance.Resolve<IServerSettings>();
            var pairingService = UnityServiceLocator.Instance.Resolve<IPairingService>();
            var creditCardDao = UnityServiceLocator.Instance.Resolve<ICreditCardDao>();
            var orderDao = UnityServiceLocator.Instance.Resolve<IOrderDao>();
            return new MonerisPaymentService(commandBus, logger, orderPaymentDao, serverSettings, serverSettings.GetPaymentSettings(), pairingService, creditCardDao, orderDao);
        }

        [Test]
        public async void when_tokenizing_a_credit_card_with_avs()
        {
            var client = GetPaymentClient();
            var visa = TestCreditCards.Visa;

            var result = await client.Tokenize(visa.Number, 
                    visa.NameOnCard, 
                    visa.ExpirationDate, 
                    visa.AvcCvvCvv2.ToString(),
                    null,
                    visa.ZipCode, 
                    TestAccount, 
                    "7250", 
                    "Mile-End", 
                    TestAccount.Email, 
                    TestAccount.Phone);


            Assert.IsTrue(result.IsSuccessful);
        }

        [Test]
        public async void when_paying_fails_cvd_invalid()
        {
            var preauthResponse = await GetPreauthResponse((decimal)10.35);

            Assert.IsFalse(preauthResponse.IsSuccessful);
        }

        async Task<PreAuthorizePaymentResponse> GetPreauthResponse(decimal amountToPreauth)
        {
            var orderId = Guid.NewGuid();
            var visa = TestCreditCards.Visa;


            
            var tokenizedResult = await GetPaymentClient().Tokenize("4242424242424242",
                visa.NameOnCard,
                visa.ExpirationDate,
                visa.AvcCvvCvv2.ToString(),
                null,
                visa.ZipCode,
                TestAccount,
                "7250",
                "Mile-End",
                TestAccount.Email,
                "5145552222");


            if (tokenizedResult == null || !tokenizedResult.IsSuccessful)
            {
                Assert.Fail("Tokenization failed");
            }

            var ccId = Guid.NewGuid();

            using (var context = ContextFactory.Invoke())
            {
                context.Set<CreditCardDetails>().Add(new CreditCardDetails()
                       {
                    Label = string.Empty,
                    AccountId = TestAccount.Id,
                    Country = new CountryISOCode("CA"),
                    Phone = "5145552222",
                    CreditCardCompany = "Visa",
                    NameOnCard = TestAccount.Name,
                    StreetNumber = "7250",
                    StreetName = "Mile-End",
                    CreditCardId = ccId,
                    Email = TestAccount.Email,
                    ExpirationMonth = visa.ExpirationDate.Month.ToString(),
                    ExpirationYear = visa.ExpirationDate.Year.ToString(),
                    Last4Digits = "4242",
                    ZipCode = visa.ZipCode,
                    Token = tokenizedResult.CardOnFileToken
                });

                context.Set<OrderDetail>().Add(new OrderDetail()
                       {
                    Id = orderId,
                    IBSOrderId = 9999252,
                    ClientLanguageCode = "en",
                    AccountId = TestAccount.Id,
                    CompanyKey = "TaxihailDemo",
                    CompanyName = "Taxihail test suite",
                    PickupAddress = new Address()
                    {
                        FullAddress = "7250 Mile-End, Montreal"
                    },
                    PickupDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    Settings = new BookingSettings()
                    {
                        ChargeTypeId = ChargeTypes.CardOnFile.Id
                    },
                    DropOffAddress = new Address()
                });

                context.SaveChanges();
            }

            var accountDetails = UnityServiceLocator.Instance.Resolve<IAccountDao>().FindById(TestAccount.Id);

            accountDetails.DefaultCreditCard = ccId;

            var preauthResponse = GetPaymentService()
                .PreAuthorize("TaxihailDemo", orderId, accountDetails, amountToPreauth, cvv: visa.AvcCvvCvv2.ToString());
            return preauthResponse;
        }

        [Test]
        public async void when_paying_fails_avs_invalid()
        {
            var preauthResponse = await GetPreauthResponse((decimal)10.37);

            Assert.IsFalse(preauthResponse.IsSuccessful);
        }

        [Test]
        public async void when_paying_succeed_cvd_avs_valid()
        {
            var preauthResponse = await GetPreauthResponse(20);

            Assert.IsTrue(preauthResponse.IsSuccessful);
        }
    }
}