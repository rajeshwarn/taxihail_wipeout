using System;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Jobs;
using apcurium.MK.Booking.Api.Providers;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Api
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            RegisterMaps();

            container.RegisterInstance<IPopularAddressProvider>(new PopularAddressProvider(container.Resolve<IPopularAddressDao>()));
            container.RegisterInstance<ITariffProvider>(new TariffProvider(container.Resolve<ITariffDao>()));
            container.RegisterType<IUpdateOrderStatusJob, UpdateOrderStatusJob>();
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
            AutoMapper.Mapper.CreateMap<PaymentSettings, Commands.CreateOrder.PaymentInformation>();
            AutoMapper.Mapper.CreateMap<BookingSettings, Commands.SendBookingConfirmationEmail.BookingSettings>();
            AutoMapper.Mapper.CreateMap<Address, IBSAddress>()
                .ForMember(x => x.FullAddress, y => y.ResolveUsing(a => a.BookAddress));

            AutoMapper.Mapper.CreateMap<RegisterAccount, Commands.RegisterAccount>()
                .ForMember(p => p.AccountId, options => options.ResolveUsing(x => x.AccountId == Guid.Empty ? Guid.NewGuid() : x.AccountId));
            AutoMapper.Mapper.CreateMap<RegisterAccount, Commands.RegisterTwitterAccount>()
                .ForMember(p => p.AccountId, options => options.ResolveUsing(x => x.AccountId == Guid.Empty ? Guid.NewGuid() : x.AccountId)); ;
            AutoMapper.Mapper.CreateMap<RegisterAccount, Commands.RegisterFacebookAccount>()
                .ForMember(p => p.AccountId, options => options.ResolveUsing(x => x.AccountId == Guid.Empty ? Guid.NewGuid() : x.AccountId)); ;

            AutoMapper.Mapper.CreateMap<SaveAddress, Commands.AddFavoriteAddress>();

            AutoMapper.Mapper.CreateMap<SaveAddress, Commands.UpdateFavoriteAddress>();

            AutoMapper.Mapper.CreateMap<DefaultFavoriteAddress, Commands.AddDefaultFavoriteAddress>();

            AutoMapper.Mapper.CreateMap<DefaultFavoriteAddress, Commands.UpdateDefaultFavoriteAddress>();

            AutoMapper.Mapper.CreateMap<AccountDetail, CurrentAccountResponse>();
            AutoMapper.Mapper.CreateMap<BookingSettingsDetails, BookingSettings>();
            AutoMapper.Mapper.CreateMap<Contract.Requests.Tariff, Commands.CreateTariff>()
                .ForMember(p => p.TariffId, opt => opt.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id))
                .ForMember(p => p.CompanyId, opt => opt.UseValue(AppConstants.CompanyId));

            AutoMapper.Mapper.CreateMap<Contract.Requests.Tariff, Commands.UpdateTariff>()
               .ForMember(p => p.TariffId, opt => opt.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id))
               .ForMember(p => p.CompanyId, opt => opt.UseValue(AppConstants.CompanyId));

  			AutoMapper.Mapper.CreateMap<CreditCardRequest, Commands.AddCreditCard>()
                .ForMember(x => x.CreditCardId, opt => opt.ResolveUsing(x => x.CreditCardId == Guid.Empty ? Guid.NewGuid() : x.CreditCardId));

            AutoMapper.Mapper.CreateMap<PopularAddress, Commands.AddPopularAddress>();
            AutoMapper.Mapper.CreateMap<PopularAddress, Commands.UpdatePopularAddress>();

        }
    }
}
