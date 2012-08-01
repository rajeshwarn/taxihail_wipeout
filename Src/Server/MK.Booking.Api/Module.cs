using Microsoft.Practices.Unity;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.OrmLite;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common.Caching;

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
            AutoMapper.Mapper.CreateMap<CreateOrder, Commands.CreateOrder>().ForMember(p => p.OrderId, options => options.MapFrom(m => m.Id));
            AutoMapper.Mapper.CreateMap<Address, Commands.CreateOrder.Address>();
            AutoMapper.Mapper.CreateMap<BookingSettings, Commands.CreateOrder.BookingSettings>();
            AutoMapper.Mapper.CreateMap<Address, IBSAddress>();
            AutoMapper.Mapper.CreateMap<RegisterAccount, Commands.RegisterAccount>();
            AutoMapper.Mapper.CreateMap<RegisterAccount, Commands.RegisterTwitterAccount>();
            AutoMapper.Mapper.CreateMap<RegisterAccount, Commands.RegisterFacebookAccount>();

            AutoMapper.Mapper.CreateMap<SaveAddress, Commands.AddFavoriteAddress>()
                .ForMember(x => x.AddressId, opt => opt.MapFrom(x => x.Id));

            AutoMapper.Mapper.CreateMap<SaveAddress, Commands.UpdateFavoriteAddress>()
                .ForMember(x => x.AddressId, opt => opt.MapFrom(x => x.Id));
        }
    }
}
