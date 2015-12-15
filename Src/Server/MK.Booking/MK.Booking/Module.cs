#region

using System.Configuration;
using System.Net;
using System.Net.Mail;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.EventHandlers.Integration;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.PushNotifications.Impl;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Booking.Services;
using apcurium.MK.Booking.Services.Impl;
using apcurium.MK.Booking.SMS;
using apcurium.MK.Booking.SMS.Impl;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using AutoMapper;
using CMTServices;
using CustomerPortal.Client;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using Microsoft.Practices.Unity;

#endregion

namespace apcurium.MK.Booking
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            container.RegisterType<IUpdateOrderStatusJob>(
                new TransientLifetimeManager(),
                new InjectionFactory(c =>
                {
                    var serverSettings = c.Resolve<IServerSettings>();
                    if (serverSettings.ServerData.IBS.FakeOrderStatusUpdate)
                    {
                        return new UpdateOrderStatusJobStub(c.Resolve<IOrderDao>(), c.Resolve<IOrderStatusUpdateDao>(), c.Resolve<OrderStatusUpdater>());
                    }

                    return new UpdateOrderStatusJob(c.Resolve<IOrderDao>(), c.Resolve<IIBSServiceProvider>(), c.Resolve<IOrderStatusUpdateDao>(), c.Resolve<OrderStatusUpdater>(), c.Resolve<HoneyBadgerServiceClient>(), c.Resolve<IServerSettings>());
                }));

            System.Data.Entity.Database.SetInitializer<BookingDbContext>(null);
            container.RegisterType<ISmsService, TwilioService>();

            container.RegisterInstance<IConfigurationDao>(new ConfigurationDao(() => container.Resolve<ConfigurationDbContext>()));
            container.RegisterType<BookingDbContext>(new TransientLifetimeManager(),
                new InjectionConstructor(
                    container.Resolve<ConnectionStringSettings>(Common.Module.MkConnectionString).ConnectionString));

            container.RegisterInstance<IEmailSender>(new EmailSender(container.Resolve<IServerSettings>()));
            container.RegisterInstance<ITemplateService>(new TemplateService(container.Resolve<IServerSettings>()));
            container.RegisterInstance<IPushNotificationService>(new PushNotificationService(container.Resolve<IServerSettings>(), container.Resolve<ILogger>()));
            container.RegisterInstance<IOrderDao>(new OrderDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IReportDao>(new ReportDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IPromotionDao>(new PromotionDao(() => container.Resolve<BookingDbContext>(), container.Resolve<IClock>(), container.Resolve<IServerSettings>(), container.Resolve<IEventSourcedRepository<Promotion>>()));
            container.RegisterType<INotificationService, NotificationService>(new ContainerControlledLifetimeManager());
                    
            container.RegisterType<IPairingService>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => new PairingService(c.Resolve<ICommandBus>(), c.Resolve<IIbsOrderService>(), c.Resolve<IOrderDao>(), c.Resolve<IServerSettings>())));

            container.RegisterType<IIbsCreateOrderService>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => new IbsCreateOrderService(c.Resolve<IServerSettings>(),
                    c.Resolve<IVehicleTypeDao>(),
                    c.Resolve<IAccountDao>(),
                    c.Resolve<ILogger>(),
                    c.Resolve<IIBSServiceProvider>(),
                    c.Resolve<IUpdateOrderStatusJob>(),
                    c.Resolve<IDispatcherService>())));

            container.RegisterInstance<IAddressDao>(new AddressDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IAccountDao>(new AccountDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IFeesDao>(new FeesDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IOrderStatusUpdateDao>(new OrderStatusUpdateDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IDefaultAddressDao>(new DefaultAddressDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<ITariffDao>(new TariffDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IRuleDao>(new RuleDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IOrderRatingsDao>(new OrderRatingsDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IRatingTypeDao>(new RatingTypeDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IPopularAddressDao>(new PopularAddressDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<ICreditCardDao>(new CreditCardDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IOrderPaymentDao>(new OrderPaymentDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IDeviceDao>(new DeviceDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<ICompanyDao>(new CompanyDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IAccountChargeDao>(new AccountChargeDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IVehicleTypeDao>(new VehicleTypeDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IAppStartUpLogDao>(new AppStartUpLogDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IPasswordService>(new PasswordService());
            container.RegisterInstance<IRuleCalculator>(new RuleCalculator(container.Resolve<IRuleDao>(), container.Resolve<IServerSettings>()));
            container.RegisterInstance<IOverduePaymentDao>(new OverduePaymentDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IOrderNotificationsDetailDao>(new OrderNotificationsDetailDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IBlackListEntryService>(new BlackListEntryService(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IAirlineDao>(new AirlineDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IPickupPointDao>(new PickupPointDao(() => container.Resolve<BookingDbContext>()));
            
            RegisterMaps();
            RegisterCommandHandlers(container);
            RegisterEventHandlers(container);

            container.RegisterType<IPayPalServiceFactory, PayPalServiceFactory>();
            container.RegisterType<IPaymentService, PaymentService>();
            container.RegisterType<IDispatcherService, DispatcherService>();

            container.RegisterType<IFeeService, FeeService>();
        }

        public void RegisterMaps()
        {
            Mapper.CreateMap<UpdateBookingSettings, BookingSettings>();
            Mapper.CreateMap<CreateOrder.PaymentInformation, PaymentInformation>();
	        Mapper.CreateMap<Address, AddressDetails>()
		        .ForMember(x => x.PlaceReference, opt => opt.MapFrom(x => x.PlaceId));

            Mapper.CreateMap<EmailSender.SmtpConfiguration, SmtpClient>()
                .ForMember(x => x.Credentials, opt => opt.MapFrom(x => new NetworkCredential(x.Username, x.Password)));

            Mapper.CreateMap<FavoriteAddressAdded, AddressDetails>()
                .ForMember(p => p.AccountId, opt => opt.MapFrom(m => m.SourceId));

            Mapper.CreateMap<FavoriteAddressUpdated, AddressDetails>();

            Mapper.CreateMap<Address, DefaultAddressDetails>();

            Mapper.CreateMap<Address, PopularAddressDetails>()
				.ForMember(x => x.PlaceReference, opt => opt.MapFrom(x => x.PlaceId))
				.ForMember(x => x.AddressLocationType, opt => opt.MapFrom(x => (int)x.AddressLocationType));

            Mapper.CreateMap<PopularAddressDetails, Address>()
				.ForMember(x => x.PlaceId, opt => opt.MapFrom(x => x.PlaceReference))
				.ForMember(x => x.AddressLocationType, opt => opt.MapFrom(x => x.AddressLocationType)); ; 

            Mapper.CreateMap<TariffDetail, Tariff>();
            Mapper.CreateMap<RuleDetail, Rule>();
            Mapper.CreateMap<CreditCardAddedOrUpdated, CreditCardDetails>()
                .ForMember(p => p.AccountId, opt => opt.MapFrom(m => m.SourceId));

            Mapper.CreateMap<OrderStatusDetail, OrderStatusDetail>();
        }

        private static void RegisterEventHandlers(IUnityContainer container)
        {
            container.RegisterType<IEventHandler, AccountDetailsGenerator>("AccountDetailsGenerator");   
            container.RegisterType<IEventHandler, AddressListGenerator>("AddressListGenerator");
            container.RegisterType<IEventHandler, OrderGenerator>("OrderGenerator");
            container.RegisterType<IEventHandler, ReportDetailGenerator>("ReportDetailGenerator");
            container.RegisterType<IEventHandler, TariffDetailsGenerator>("TariffDetailsGenerator");
            container.RegisterType<IEventHandler, RuleDetailsGenerator>("RuleDetailsGenerator");
            container.RegisterType<IEventHandler, RatingTypeDetailsGenerator>("RatingTypeDetailsGenerator");
            container.RegisterType<IEventHandler, AppSettingsGenerator>("AppSettingsGenerator");
            container.RegisterType<IEventHandler, CreditCardDetailsGenerator>("CreditCardDetailsGenerator");
            container.RegisterType<IEventHandler, PaymentSettingGenerator>(typeof (PaymentSettingGenerator).Name);
            container.RegisterType<IEventHandler, CreditCardPaymentDetailsGenerator>("CreditCardPaymentDetailsGenerator");
            container.RegisterType<IEventHandler, CompanyDetailsGenerator>("CompanyDetailsGenerator");
            container.RegisterType<IEventHandler, OrderUserGpsGenerator>("OrderUserGpsGenerator");
            container.RegisterType<IEventHandler, AccountChargeDetailGenerator>("AccountChargeDetailGenerator");
            container.RegisterType<IEventHandler, VehicleTypeDetailGenerator>("VehicleTypeDetailGenerator");
            container.RegisterType<IEventHandler, NotificationSettingsGenerator>("NotificationSettingsGenerator");
            container.RegisterType<IEventHandler, UserTaxiHailNetworkSettingsGenerator>("TaxiHailNetworkSettingsGenerator");
            container.RegisterType<IEventHandler, PromotionDetailGenerator>("PromotionDetailGenerator");
            container.RegisterType<IEventHandler, PromotionTriggerGenerator>("PromotionTriggerGenerator");
            container.RegisterType<IEventHandler, OverduePaymentDetailGenerator>("OverduePaymentDetailGenerator");
            container.RegisterType<IEventHandler, FeesDetailsGenerator>("FeesDetailsGenerator");

            // Integration event handlers
            container.RegisterType<IEventHandler, PushNotificationSender>("PushNotificationSender");
            container.RegisterType<IEventHandler, PaymentSettingsUpdater>(typeof (PaymentSettingsUpdater).Name);
            container.RegisterType<IEventHandler, MailSender>("MailSender");
            container.RegisterType<IEventHandler, OrderPaymentManager>("OrderPaymentManager");
            container.RegisterType<IEventHandler, OrderPairingManager>("OrderPairingManager");
            container.RegisterType<IEventHandler, OrderDispatchCompanyManager>("OrderDispatchCompanyManager");
            container.RegisterType<IEventHandler, CacheServiceManager>("CacheServiceManager");
            container.RegisterType<IEventHandler, OrderCreationManager>("OrderCreationManager");
        }

        private void RegisterCommandHandlers(IUnityContainer container)
        {
            container.RegisterType<ICommandHandler, AccountCommandHandler>("AccountCommandHandler");
            container.RegisterType<ICommandHandler, EmailCommandHandler>("EmailCommandHandler");
            container.RegisterType<ICommandHandler, OrderCommandHandler>("OrderCommandHandler");
            container.RegisterType<ICommandHandler, CompanyCommandHandler>("CompanyCommandHandler");
            container.RegisterType<ICommandHandler, CreditCardPaymentCommandHandler>("CreditCardPaymentCommandHandler");
            container.RegisterType<ICommandHandler, SmsCommandHandler>("SmsCommandHandler");
            container.RegisterType<ICommandHandler, PromotionCommandHandler>("PromotionCommandHandler");
            container.RegisterType<ICommandHandler, NonDomainRelatedCommandHandler>("NonDomainRelatedCommandHandler");
        }
    }
}