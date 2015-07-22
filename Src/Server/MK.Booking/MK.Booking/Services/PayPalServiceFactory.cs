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

        public PayPalService GetInstance(string companyKey = null)
        {
            var serverSettings = _container.Resolve<IServerSettings>();
            var paymentSettings = serverSettings.GetPaymentSettings(companyKey);

            return new PayPalService(serverSettings, paymentSettings, _container.Resolve<ICommandBus>(),
                _container.Resolve<IAccountDao>(), _container.Resolve<IOrderDao>(), _container.Resolve<ILogger>(),
                _container.Resolve<IPairingService>(), _container.Resolve<IOrderPaymentDao>());
        }
    }
}
