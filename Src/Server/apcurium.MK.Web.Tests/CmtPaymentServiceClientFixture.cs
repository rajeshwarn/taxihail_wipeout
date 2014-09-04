using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.CmtPayments;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.IoC;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web.Tests
{
    [TestFixture]
    [Ignore("Tests broken, one day CMT will be more helpful")]
    public class CmtPaymentServiceClientFixture : BasePaymentClientFixture
    {
        public CmtPaymentServiceClientFixture() : base(TestCreditCards.TestCreditCardSetting.Cmt)
        {
        }

        public override void Setup()
        {
            base.Setup();
            UnityServiceLocator.Instance.RegisterInstance<IPaymentService>(GetPaymentService());
        }

        protected override IPaymentServiceClient GetPaymentClient()
        {
            return new CmtPaymentClient(BaseUrl, SessionId, new CmtPaymentSettings(), new DummyPackageInfo(), null);
        }

        protected override PaymentProvider GetProvider()
        {
            return PaymentProvider.Cmt;
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
            return new CmtPaymentService(commandBus, orderDao, logger, ibsOrderService, accountDao, orderPaymentDao, configManager, pairingService);
        }
    }
}