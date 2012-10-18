using System;
using Microsoft.Practices.Unity;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            RegisterMaps();
        }

        private void RegisterMaps()
        {
            AutoMapper.Mapper.CreateMap<BookingSettingsRequest, Commands.UpdateBookingSettings>();
            AutoMapper.Mapper.CreateMap<CreateOrder, Commands.CreateOrder>()
                .ForMember(p=> p.Id, options=> options.Ignore())
                .ForMember(p => p.OrderId, options => options.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id));

            AutoMapper.Mapper.CreateMap<apcurium.MK.Booking.Api.Contract.Requests.CreateOrder, Commands.SendBookingConfirmationEmail>()
                .ForMember(p => p.Id, options => options.Ignore());

            AutoMapper.Mapper.CreateMap<BookingSettings, Commands.CreateOrder.BookingSettings>();
            AutoMapper.Mapper.CreateMap<BookingSettings, Commands.SendBookingConfirmationEmail.BookingSettings>();
            AutoMapper.Mapper.CreateMap<Address, IBSAddress>();
            AutoMapper.Mapper.CreateMap<RegisterAccount, Commands.RegisterAccount>()
                .ForMember(p => p.AccountId, options => options.ResolveUsing(x => x.AccountId == Guid.Empty ? Guid.NewGuid() : x.AccountId));
            AutoMapper.Mapper.CreateMap<RegisterAccount, Commands.RegisterTwitterAccount>()
                .ForMember(p => p.AccountId, options => options.ResolveUsing(x => x.AccountId == Guid.Empty ? Guid.NewGuid() : x.AccountId)); ;
            AutoMapper.Mapper.CreateMap<RegisterAccount, Commands.RegisterFacebookAccount>()
                .ForMember(p => p.AccountId, options => options.ResolveUsing(x => x.AccountId == Guid.Empty ? Guid.NewGuid() : x.AccountId)); ;

            AutoMapper.Mapper.CreateMap<SaveAddress, Commands.AddFavoriteAddress>()
                .ForMember(x => x.AddressId, opt => opt.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id));

            AutoMapper.Mapper.CreateMap<SaveAddress, Commands.UpdateFavoriteAddress>()
                .ForMember(x => x.AddressId, opt => opt.MapFrom(x => x.Id));

            AutoMapper.Mapper.CreateMap<DefaultFavoriteAddress, Commands.AddDefaultFavoriteAddress>()
               .ForMember(x => x.AddressId, opt => opt.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id));

            AutoMapper.Mapper.CreateMap<DefaultFavoriteAddress, Commands.UpdateDefaultFavoriteAddress>()
                .ForMember(x => x.AddressId, opt => opt.MapFrom(x => x.Id));
        }
    }
}
