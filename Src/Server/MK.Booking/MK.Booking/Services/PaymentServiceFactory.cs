using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;

namespace apcurium.MK.Booking.Services
{
    class PaymentServiceFactory : IPaymentServiceFactory
    {
        private readonly IUnityContainer _container;

        public PaymentServiceFactory(IUnityContainer container)
        {
            _container = container;
        }

        public IPaymentService GetInstance()
        {
            var serverSettings = _container.Resolve<IServerSettings>();
            switch (serverSettings.GetPaymentSettings().PaymentMode)
            {
                case PaymentMethod.Braintree:
                    return new BraintreePaymentService(_container.Resolve<ICommandBus>(), _container.Resolve<ILogger>(), _container.Resolve<IOrderPaymentDao>(), serverSettings, _container.Resolve<IPairingService>());
                case PaymentMethod.RideLinqCmt:
                case PaymentMethod.Cmt:
                    return new CmtPaymentService(_container.Resolve<ICommandBus>(), _container.Resolve<IOrderDao>(), _container.Resolve<ILogger>(), _container.Resolve<IAccountDao>(), _container.Resolve<IOrderPaymentDao>(), serverSettings, _container.Resolve<IPairingService>());
                case PaymentMethod.Moneris:
                    return new MonerisPaymentService(_container.Resolve<ICommandBus>(), _container.Resolve<ILogger>(), _container.Resolve<IOrderPaymentDao>(), serverSettings, _container.Resolve<IPairingService>());
                default:
                    return null;
            }
        }
    }
}