#region

using System.Collections;
using System.Collections.Generic;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using CustomerPortal.Client;
using CustomerPortal.Client.Impl;
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

            var thNetworkServiceClient = new TaxiHailNetworkServiceClient(container.Resolve<IServerSettings>());
            container.RegisterInstance<ITaxiHailNetworkServiceClient>(thNetworkServiceClient);

            container.RegisterInstance<IIBSServiceProvider>(
                new IBSServiceProvider(container.Resolve<IServerSettings>(), container.Resolve<ILogger>(),container.Resolve<ITaxiHailNetworkServiceClient>()));
        }
    }
}