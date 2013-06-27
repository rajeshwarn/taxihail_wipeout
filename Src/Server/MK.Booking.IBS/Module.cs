using AutoMapper;
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
            var profile = new IBSAutoMapperProfile();
            Mapper.AddProfile(profile);
            Mapper.AssertConfigurationIsValid(profile.ProfileName);

            container.RegisterType<IAccountWebServiceClient,AccountWebServiceClient>(new ContainerControlledLifetimeManager());
            container.RegisterType<IStaticDataWebServiceClient, StaticDataWebServiceClient>(new ContainerControlledLifetimeManager());
            container.RegisterType<IBookingWebServiceClient, BookingWebServiceClient>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDriverWebServiceClient, DriverWebServiceClient>(new ContainerControlledLifetimeManager());
            //, container.Resolve<IStaticDataWebServiceClient>()
            //container.RegisterInstance<IStaticDataWebServiceClient>(new StaticDataWebServiceClient(container.Resolve<IConfigurationManager>(), new Logger()));
            //container.RegisterInstance<IBookingWebServiceClient>(new BookingWebServiceClient(container.Resolve<IConfigurationManager>(), new Logger()));
        }

    }

}
