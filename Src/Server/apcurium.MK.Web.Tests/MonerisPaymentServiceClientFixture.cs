using System.Linq;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.Moneris;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
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
            UnityServiceLocator.Instance.RegisterInstance<IPaymentService>(GetPaymentService());
        }

        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new MonerisServiceClient(BaseUrl, SessionId, new MonerisPaymentSettings(), new DummyPackageInfo(), new Logger());
        }

        protected override PaymentProvider GetProvider()
        {
            return PaymentProvider.Moneris;
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
            return new MonerisPaymentService(commandBus, orderDao, logger, ibsOrderService, accountDao, orderPaymentDao, configManager);
        }
    }
}