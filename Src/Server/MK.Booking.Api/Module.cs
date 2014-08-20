#region

using System;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Jobs;
using apcurium.MK.Booking.Api.Payment;
using apcurium.MK.Booking.Api.Providers;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;
using AutoMapper;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using CreateOrder = apcurium.MK.Booking.Api.Contract.Requests.CreateOrder;
using RegisterAccount = apcurium.MK.Booking.Api.Contract.Requests.RegisterAccount;
using Tariff = apcurium.MK.Booking.Api.Contract.Requests.Tariff;

#endregion

namespace apcurium.MK.Booking.Api
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            RegisterMaps();

            container.RegisterInstance<IPopularAddressProvider>(
                new PopularAddressProvider(container.Resolve<IPopularAddressDao>()));
            container.RegisterInstance<ITariffProvider>(new TariffProvider(container.Resolve<ITariffDao>()));

            container.RegisterType<ExpressCheckoutServiceFactory, ExpressCheckoutServiceFactory>();
            container.RegisterType<IIbsOrderService, IbsOrderService>();

            container.RegisterType<OrderStatusUpdater, OrderStatusUpdater>();

            container.RegisterType<IUpdateOrderStatusJob>(
                new TransientLifetimeManager(),
                new InjectionFactory(c =>
                {
                    var configManager = c.Resolve<IConfigurationManager>();
                    var mockIbsStatusUpdate = bool.Parse(configManager.GetSetting("IBS.FakeOrderStatusUpdate") ?? "false");
                    if (mockIbsStatusUpdate)
                    {
                        return new UpdateOrderStatusJobStub();
                    }
                    
                    return new UpdateOrderStatusJob(c.Resolve<IOrderDao>(), c.Resolve<IBookingWebServiceClient>(), c.Resolve<IOrderStatusUpdateDao>(), c.Resolve<OrderStatusUpdater>());
                }));

            container.RegisterType<OrderStatusHelper>(
                new TransientLifetimeManager(),
                new InjectionFactory(c =>
                {
                    var configManager = c.Resolve<IConfigurationManager>();
                    var mockIbsStatusUpdate = bool.Parse(configManager.GetSetting("IBS.FakeOrderStatusUpdate") ?? "false");
                    return mockIbsStatusUpdate 
                        ? new OrderStatusIbsMock(c.Resolve<IOrderDao>(), c.Resolve<OrderStatusUpdater>(), configManager) 
                        : new OrderStatusHelper(c.Resolve<IOrderDao>(), configManager);
                }));

            container.RegisterType<IPaymentService>(
                new TransientLifetimeManager(),
                new InjectionFactory(c =>
                {
                    var configManager = c.Resolve<IConfigurationManager>();
                    switch (configManager.GetPaymentSettings().PaymentMode)
                    {
                        case PaymentMethod.Braintree:
                            return new BraintreePaymentService(c.Resolve<ICommandBus>(), c.Resolve<IOrderDao>(), c.Resolve<ILogger>(), c.Resolve<IIbsOrderService>(), c.Resolve<IAccountDao>(), c.Resolve<IOrderPaymentDao>(), configManager);
                        case PaymentMethod.RideLinqCmt:
                        case PaymentMethod.Cmt:
                            return new CmtPaymentService(c.Resolve<ICommandBus>(), c.Resolve<IOrderDao>(), c.Resolve<ILogger>(), c.Resolve<IIbsOrderService>(), c.Resolve<IAccountDao>(), c.Resolve<IOrderPaymentDao>(), configManager);
                        case PaymentMethod.Moneris:
                            return new MonerisPaymentService(c.Resolve<ICommandBus>(), c.Resolve<IOrderDao>(), c.Resolve<ILogger>(), c.Resolve<IIbsOrderService>(), c.Resolve<IAccountDao>(), c.Resolve<IOrderPaymentDao>(), configManager);
                        default:
                            return null;
                    }
                }));
        }


        private void RegisterMaps()
        {
            var profile = new BookingApiMapperProfile();
            Mapper.AddProfile(profile);
            Mapper.AssertConfigurationIsValid(profile.ProfileName);

            Mapper.CreateMap<BookingSettingsRequest, UpdateBookingSettings>();
            Mapper.CreateMap<CreateOrder, Commands.CreateOrder>()
                .ForMember(p => p.Id, options => options.Ignore())
                .ForMember(p => p.EstimatedFare, opt => opt.ResolveUsing(x => x.Estimate.Price))
                .ForMember(p => p.OrderId,
                    options => options.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id));

            Mapper.CreateMap<CreateOrder, SendBookingConfirmationEmail>()
                .ForMember(p => p.Id, options => options.Ignore());

            Mapper.CreateMap<PaymentSettings, Commands.CreateOrder.PaymentInformation>();
            Mapper.CreateMap<BookingSettings, SendBookingConfirmationEmail.BookingSettings>();
            Mapper.CreateMap<Address, IbsAddress>()
                .ForMember(x => x.FullAddress, y => y.ResolveUsing(a => a.BookAddress));

            Mapper.CreateMap<OrderStatusDetail, OrderStatusRequestResponse>();
            Mapper.CreateMap<OrderPairingDetail, OrderPairingResponse>();

            Mapper.CreateMap<RegisterAccount, Commands.RegisterAccount>()
                .ForMember(p => p.AccountId,
                    options => options.ResolveUsing(x => x.AccountId == Guid.Empty ? Guid.NewGuid() : x.AccountId));
            Mapper.CreateMap<RegisterAccount, RegisterTwitterAccount>()
                .ForMember(p => p.AccountId,
                    options => options.ResolveUsing(x => x.AccountId == Guid.Empty ? Guid.NewGuid() : x.AccountId));
            
            Mapper.CreateMap<RegisterAccount, RegisterFacebookAccount>()
                .ForMember(p => p.AccountId,
                    options => options.ResolveUsing(x => x.AccountId == Guid.Empty ? Guid.NewGuid() : x.AccountId));
            

            Mapper.CreateMap<SaveAddress, AddFavoriteAddress>();

            Mapper.CreateMap<SaveAddress, UpdateFavoriteAddress>();

            Mapper.CreateMap<DefaultFavoriteAddress, AddDefaultFavoriteAddress>();

            Mapper.CreateMap<DefaultFavoriteAddress, UpdateDefaultFavoriteAddress>();

            Mapper.CreateMap<AccountDetail, CurrentAccountResponse>()
                .ForMember(x => x.IsSuperAdmin, opt => opt.ResolveUsing(x => x.RoleNames.Contains(RoleName.SuperAdmin)));


            Mapper.CreateMap<Tariff, CreateTariff>()
                .ForMember(p => p.TariffId, opt => opt.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id))
                .ForMember(p => p.CompanyId, opt => opt.UseValue(AppConstants.CompanyId));

            Mapper.CreateMap<Tariff, UpdateTariff>()
                .ForMember(p => p.TariffId, opt => opt.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id))
                .ForMember(p => p.CompanyId, opt => opt.UseValue(AppConstants.CompanyId));


            Mapper.CreateMap<RuleRequest, CreateRule>()
                .ForMember(p => p.RuleId, opt => opt.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id))
                .ForMember(p => p.CompanyId, opt => opt.UseValue(AppConstants.CompanyId));

            Mapper.CreateMap<RuleRequest, UpdateRule>()
                .ForMember(p => p.RuleId, opt => opt.ResolveUsing(x => x.Id == Guid.Empty ? Guid.NewGuid() : x.Id))
                .ForMember(p => p.CompanyId, opt => opt.UseValue(AppConstants.CompanyId));

            Mapper.CreateMap<RuleActivateRequest, ActivateRule>()
                .ForMember(p => p.CompanyId, opt => opt.UseValue(AppConstants.CompanyId));

            Mapper.CreateMap<RuleDeactivateRequest, DeactivateRule>()
                .ForMember(p => p.CompanyId, opt => opt.UseValue(AppConstants.CompanyId));

            Mapper.CreateMap<CreditCardRequest, AddCreditCard>()
                .ForMember(x => x.CreditCardId,
                    opt => opt.ResolveUsing(x => x.CreditCardId == Guid.Empty ? Guid.NewGuid() : x.CreditCardId));

            Mapper.CreateMap<CreditCardRequest, UpdateCreditCard>();

            Mapper.CreateMap<PopularAddress, AddPopularAddress>();
            Mapper.CreateMap<PopularAddress, UpdatePopularAddress>();
        }
    }

    public class BookingApiMapperProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<IbsVehiclePosition, AvailableVehicle>()
                .ForMember(p => p.VehicleNumber, opt => opt.ResolveUsing(x => x.VehicleNumber));
        }
    }
}