using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;

namespace apcurium.MK.Booking.Services
{
    public class PayPalServiceFactory : IPayPalServiceFactory
    {
        private readonly IUnityContainer _container;

        public PayPalServiceFactory(IUnityContainer container)
        {
            _container = container;
        }

        public PayPalService GetInstance()
        {
            return new PayPalService(_container.Resolve<IServerSettings>(), _container.Resolve<ICommandBus>(),
                _container.Resolve<IAccountDao>(), _container.Resolve<ILogger>(),_container.Resolve<IPairingService>(), 
                _container.Resolve<IOrderPaymentDao>());
        }
    }
}
