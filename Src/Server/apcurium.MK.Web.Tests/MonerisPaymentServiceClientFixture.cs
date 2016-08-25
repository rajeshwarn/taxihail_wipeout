using System.Linq;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.Moneris;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using NUnit.Framework;
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
    }
}