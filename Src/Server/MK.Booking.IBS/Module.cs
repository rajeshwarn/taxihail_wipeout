﻿#region

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

            container.RegisterType<IIBSServiceProvider>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => new IBSServiceProvider(c.Resolve<IServerSettings>(), c.Resolve<ILogger>(), c.Resolve<ITaxiHailNetworkServiceClient>())));

        }
    }
}