#region

using apcurium.MK.Booking.IBS.Impl;
using AutoMapper;
using Microsoft.Practices.Unity;

#endregion

namespace apcurium.MK.Booking.IBS
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            var profile = new IbsAutoMapperProfile();
            Mapper.AddProfile(profile);
            Mapper.AssertConfigurationIsValid(profile.ProfileName);

            container.RegisterType<IAccountWebServiceClient, AccountWebServiceClient>(
                new ContainerControlledLifetimeManager());
            container.RegisterType<IStaticDataWebServiceClient, StaticDataWebServiceClient>(
                new ContainerControlledLifetimeManager());
            container.RegisterType<IBookingWebServiceClient, BookingWebServiceClient>(
                new ContainerControlledLifetimeManager());
        }
    }
}