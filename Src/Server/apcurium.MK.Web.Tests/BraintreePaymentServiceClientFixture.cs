using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.IoC;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.Braintree;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    public class BraintreePaymentServiceClientFixture : BasePaymentClientFixture
    {
        public BraintreePaymentServiceClientFixture() : base(TestCreditCards.TestCreditCardSetting.Braintree)
        {
        }

        public override void Setup()
        {
            base.Setup();
            UnityServiceLocator.Instance.RegisterInstance<IPaymentService>(GetPaymentService());
        }

        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new BraintreeServiceClient(BaseUrl, SessionId, new BraintreeClientSettings().ClientKey, new DummyPackageInfo());
        }

        protected override PaymentProvider GetProvider()
        {
            return PaymentProvider.Braintree;
        }

        private IPaymentService GetPaymentService()
        {
            var commandBus = UnityServiceLocator.Instance.Resolve<ICommandBus>();
            var orderDao = UnityServiceLocator.Instance.Resolve<IOrderDao>();
            var logger = UnityServiceLocator.Instance.Resolve<ILogger>();
            var ibsOrderService = UnityServiceLocator.Instance.Resolve<IIbsOrderService>();
            var accountDao = UnityServiceLocator.Instance.Resolve<IAccountDao>();
            var orderPaymentDao = UnityServiceLocator.Instance.Resolve<IOrderPaymentDao>();
            var configManager = UnityServiceLocator.Instance.Resolve<IConfigurationManager>();
            var pairingService = UnityServiceLocator.Instance.Resolve<IPairingService>();
            return new BraintreePaymentService(commandBus, orderDao, logger, ibsOrderService, accountDao, orderPaymentDao, configManager, pairingService);
        }
    }
}