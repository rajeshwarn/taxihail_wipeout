using System;
using AutoMapper;
using Microsoft.Practices.Unity;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Providers;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
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
        }

        private void RegisterMaps()
        {
            Mapper.CreateMap<BookingSettingsRequest, Commands.UpdateBookingSettings>();
            Mapper.CreateMap<CreateOrder, Commands.CreateOrder>()
                .ForMember(p=> p.Id, options=> options.Ignore())
                .ForMember(p => p.OrderId, options => options.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id));

            Mapper.CreateMap<apcurium.MK.Booking.Api.Contract.Requests.CreateOrder, Commands.SendBookingConfirmationEmail>()
                .ForMember(p => p.Id, options => options.Ignore());

            Mapper.CreateMap<BookingSettings, Commands.CreateOrder.BookingSettings>();
            Mapper.CreateMap<PaymentSettings, Commands.CreateOrder.PaymentInformation>();
            Mapper.CreateMap<BookingSettings, Commands.SendBookingConfirmationEmail.BookingSettings>();
            Mapper.CreateMap<Address, IBSAddress>()
                .ForMember(x => x.FullAddress, y => y.ResolveUsing(a => a.BookAddress));

            Mapper.CreateMap<OrderStatusDetail, OrderStatusRequestResponse>();


            Mapper.CreateMap<RegisterAccount, Commands.RegisterAccount>()
                .ForMember(p => p.AccountId, options => options.ResolveUsing(x => x.AccountId == Guid.Empty ? Guid.NewGuid() : x.AccountId));
            Mapper.CreateMap<RegisterAccount, Commands.RegisterTwitterAccount>()
                .ForMember(p => p.AccountId, options => options.ResolveUsing(x => x.AccountId == Guid.Empty ? Guid.NewGuid() : x.AccountId)); ;
            Mapper.CreateMap<RegisterAccount, Commands.RegisterFacebookAccount>()
                .ForMember(p => p.AccountId, options => options.ResolveUsing(x => x.AccountId == Guid.Empty ? Guid.NewGuid() : x.AccountId)); ;

            Mapper.CreateMap<SaveAddress, Commands.AddFavoriteAddress>();

            Mapper.CreateMap<SaveAddress, Commands.UpdateFavoriteAddress>();

            Mapper.CreateMap<DefaultFavoriteAddress, Commands.AddDefaultFavoriteAddress>();

            Mapper.CreateMap<DefaultFavoriteAddress, Commands.UpdateDefaultFavoriteAddress>();

            Mapper.CreateMap<AccountDetail, CurrentAccountResponse>();
            Mapper.CreateMap<BookingSettingsDetails, BookingSettings>();
            Mapper.CreateMap<Contract.Requests.Tariff, Commands.CreateTariff>()
                .ForMember(p => p.TariffId, opt => opt.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id))
                .ForMember(p => p.CompanyId, opt => opt.UseValue(AppConstants.CompanyId));

            Mapper.CreateMap<Contract.Requests.Tariff, Commands.UpdateTariff>()
               .ForMember(p => p.TariffId, opt => opt.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id))
               .ForMember(p => p.CompanyId, opt => opt.UseValue(AppConstants.CompanyId));

  			Mapper.CreateMap<CreditCardRequest, Commands.AddCreditCard>()
                .ForMember(x => x.CreditCardId, opt => opt.ResolveUsing(x => x.CreditCardId == Guid.Empty ? Guid.NewGuid() : x.CreditCardId));

            Mapper.CreateMap<PopularAddress, Commands.AddPopularAddress>();
            Mapper.CreateMap<PopularAddress, Commands.UpdatePopularAddress>();

        }
    }
}
