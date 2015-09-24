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

            container.RegisterType<IIBSServiceProvider, IBSServiceProvider>();

            RegisterMaps();
        }

        private void RegisterMaps()
        {
            Mapper.CreateMap<IbsOrderKey, TBookOrderKey>()
                .ForMember(p => p.GUID, opt => opt.ResolveUsing(x => x.TaxiHailOrderId))
                .ForMember(p => p.OrderID, opt => opt.ResolveUsing(x => x.IbsOrderId));

            Mapper.CreateMap<TVehicleComp, IbsVehicleCandidate>()
                .ForMember(p => p.CandidateType, opt => opt.ResolveUsing(x => x.VehicleCompType));

            Mapper.CreateMap<IbsVehicleCandidate, TVehicleComp>()
                .ForMember(p => p.VehicleCompType, opt => opt.ResolveUsing(x => x.CandidateType));
        }
    }
}