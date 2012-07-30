using Microsoft.Practices.Unity;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.IBS
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            container.RegisterInstance<IAccountWebServiceClient>(new AccountWebServiceClient(container.Resolve<IConfigurationManager>(), new Logger()));
            container.RegisterInstance<IStaticDataWebServiceClient>(new StaticDataWebServiceClient(container.Resolve<IConfigurationManager>(), new Logger()));
            container.RegisterInstance<IBookingWebServiceClient>(new BookingWebServiceClient(container.Resolve<IConfigurationManager>(), new Logger()));
        }
    }
}
