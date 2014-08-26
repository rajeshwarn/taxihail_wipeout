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
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using AutoMapper;
using Infrastructure.Messaging.Handling;
using Microsoft.Practices.Unity;

#endregion

namespace apcurium.MK.Booking
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            System.Data.Entity.Database.SetInitializer<BookingDbContext>(null);
            container.RegisterType<ISmsService, TwilioService>();

            container.RegisterInstance<IConfigurationDao>(new ConfigurationDao(() => container.Resolve<ConfigurationDbContext>()));
            container.RegisterType<BookingDbContext>(new TransientLifetimeManager(),
                new InjectionConstructor(
                    container.Resolve<ConnectionStringSettings>(Common.Module.MkConnectionString).ConnectionString));

            container.RegisterInstance<IOrderDao>(new OrderDao(() => container.Resolve<BookingDbContext>(), container.Resolve<IPushNotificationService>(), container.Resolve<IConfigurationManager>()));

            container.RegisterInstance<IEmailSender>(new EmailSender(container.Resolve<IConfigurationManager>()));
            container.RegisterInstance<ITemplateService>(new TemplateService(container.Resolve<IConfigurationManager>()));
            container.RegisterInstance<IPushNotificationService>(new PushNotificationService(container.Resolve<IConfigurationManager>(), container.Resolve<ILogger>()));
            container.RegisterInstance<INotificationService>(new NotificationService(() => container.Resolve<BookingDbContext>(), container.Resolve<IPushNotificationService>(), container.Resolve<ITemplateService>(), container.Resolve<IEmailSender>(), container.Resolve<IConfigurationManager>(), container.Resolve<IAppSettings>(), container.Resolve<IConfigurationDao>(), container.Resolve<IOrderDao>()));

            container.RegisterInstance<IAddressDao>(new AddressDao(() => container.Resolve<BookingDbContext>()));
            container.RegisterInstance<IAccountDao>(new AccountDao(() => container.Resolve<BookingDbContext>()));
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
            container.RegisterInstance<IRuleCalculator>(new RuleCalculator(container.Resolve<IRuleDao>()));

            RegisterMaps();
            RegisterCommandHandlers(container);
            RegisterEventHandlers(container);
        }

        public void RegisterMaps()
        {
            Mapper.CreateMap<UpdateBookingSettings, BookingSettings>();
            Mapper.CreateMap<CreateOrder.PaymentInformation, PaymentInformation>();
            Mapper.CreateMap<Address, AddressDetails>();

            Mapper.CreateMap<EmailSender.SmtpConfiguration, SmtpClient>()
                .ForMember(x => x.Credentials, opt => opt.MapFrom(x => new NetworkCredential(x.Username, x.Password)));

            Mapper.CreateMap<FavoriteAddressAdded, AddressDetails>()
                .ForMember(p => p.AccountId, opt => opt.MapFrom(m => m.SourceId));

            Mapper.CreateMap<FavoriteAddressUpdated, AddressDetails>();

            Mapper.CreateMap<Address, DefaultAddressDetails>();

            Mapper.CreateMap<Address, PopularAddressDetails>();
            Mapper.CreateMap<PopularAddressDetails, Address>();
            Mapper.CreateMap<TariffDetail, Tariff>();
            Mapper.CreateMap<RuleDetail, Rule>();
            Mapper.CreateMap<CreditCardAdded, CreditCardDetails>()
                .ForMember(p => p.AccountId, opt => opt.MapFrom(m => m.SourceId));
            Mapper.CreateMap<CreditCardUpdated, CreditCardDetails>()
                .ForMember(p => p.AccountId, opt => opt.MapFrom(m => m.SourceId));

            Mapper.CreateMap<OrderStatusDetail, OrderStatusDetail>();

            Mapper.CreateMap<OrderDetail, OrderDetailWithAccount>()
                .ForMember(d => d.MdtFare, opt => opt.MapFrom(m => m.Fare))
                .ForMember(d => d.MdtTip, opt => opt.MapFrom(m => m.Tip))
                .ForMember(d => d.MdtToll, opt => opt.MapFrom(m => m.Toll));

            Mapper.CreateMap<AccountDetail, OrderDetailWithAccount>()
                .ForMember(d => d.Name, opt => opt.MapFrom(m => m.Settings.Name))
                .ForMember(d => d.Phone, opt => opt.MapFrom(m => m.Settings.Phone));

            Mapper.CreateMap<CreditCardDetails, OrderDetailWithAccount>()
                .ForMember(d => d.AccountDefaultCardToken, opt => opt.MapFrom(m => m.Token));


            Mapper.CreateMap<OrderPaymentDetail, OrderDetailWithAccount>()
                .ForMember(d => d.PaymentMeterAmount, opt => opt.MapFrom(m => m.Meter))
                .ForMember(d => d.PaymentTotalAmount, opt => opt.MapFrom(m => m.Amount))
                .ForMember(d => d.PaymentTipAmount, opt => opt.MapFrom(m => m.Tip))
                .ForMember(d => d.PaymentType, opt => opt.MapFrom(m => m.Type))
                .ForMember(d => d.PaymentProvider, opt => opt.MapFrom(m => m.Provider));

            Mapper.CreateMap<OrderStatusDetail, OrderDetailWithAccount>()
                .ForMember(d => d.VehicleType, opt => opt.MapFrom(m => m.DriverInfos.VehicleType))
                .ForMember(d => d.VehicleColor, opt => opt.MapFrom(m => m.DriverInfos.VehicleColor))
                .ForMember(d => d.VehicleMake, opt => opt.MapFrom(m => m.DriverInfos.VehicleMake))
                .ForMember(d => d.VehicleModel, opt => opt.MapFrom(m => m.DriverInfos.VehicleModel))
                .ForMember(d => d.DriverFirstName, opt => opt.MapFrom(m => m.DriverInfos.FirstName))
                .ForMember(d => d.DriverLastName, opt => opt.MapFrom(m => m.DriverInfos.LastName))
                .ForMember(d => d.VehicleRegistration, opt => opt.MapFrom(m => m.DriverInfos.VehicleRegistration));
        }

        private static void RegisterEventHandlers(IUnityContainer container)
        {
            container.RegisterType<IEventHandler, AccountDetailsGenerator>("AccountDetailsGenerator");
            container.RegisterType<IEventHandler, DeviceDetailsGenerator>("DeviceDetailsGenerator");
            container.RegisterType<IEventHandler, AddressListGenerator>("AddressListGenerator");
            container.RegisterType<IEventHandler, OrderGenerator>("OrderGenerator");
            container.RegisterType<IEventHandler, TariffDetailsGenerator>("TariffDetailsGenerator");
            container.RegisterType<IEventHandler, RuleDetailsGenerator>("RuleDetailsGenerator");
            container.RegisterType<IEventHandler, RatingTypeDetailsGenerator>("RatingTypeDetailsGenerator");
            container.RegisterType<IEventHandler, AppSettingsGenerator>("AppSettingsGenerator");
            container.RegisterType<IEventHandler, CreditCardDetailsGenerator>("CreditCardDetailsGenerator");
            container.RegisterType<IEventHandler, PaymentSettingGenerator>(typeof (PaymentSettingGenerator).Name);
            container.RegisterType<IEventHandler, PayPalExpressCheckoutPaymentDetailsGenerator>(
                "PayPalExpressCheckoutPaymentDetailsGenerator");
            container.RegisterType<IEventHandler, CreditCardPaymentDetailsGenerator>("CreditCardPaymentDetailsGenerator");
            container.RegisterType<IEventHandler, CompanyDetailsGenerator>("CompanyDetailsGenerator");
            container.RegisterType<IEventHandler, OrderUserGpsGenerator>("OrderUserGpsGenerator");
            container.RegisterType<IEventHandler, AccountChargeDetailGenerator>("AccountChargeDetailGenerator");
            container.RegisterType<IEventHandler, VehicleTypeDetailGenerator>("VehicleTypeDetailGenerator");
            container.RegisterType<IEventHandler, NotificationSettingsGenerator>("NotificationSettingsGenerator");

            // Integration event handlers
            container.RegisterType<IEventHandler, PushNotificationSender>("PushNotificationSender");
            container.RegisterType<IEventHandler, PaymentSettingsUpdater>(typeof (PaymentSettingsUpdater).Name);
            container.RegisterType<IEventHandler, MailSender>("MailSender");
            container.RegisterType<IEventHandler, OrderPaymentManager>("OrderPaymentManager");
        }

        private void RegisterCommandHandlers(IUnityContainer container)
        {
            container.RegisterType<ICommandHandler, AccountCommandHandler>("AccountCommandHandler");
            container.RegisterType<ICommandHandler, EmailCommandHandler>("EmailCommandHandler");
            container.RegisterType<ICommandHandler, OrderCommandHandler>("OrderCommandHandler");
            container.RegisterType<ICommandHandler, CompanyCommandHandler>("CompanyCommandHandler");
            container.RegisterType<ICommandHandler, PayPalPaymentCommandHandler>("PayPalPaymentCommandHandler");
            container.RegisterType<ICommandHandler, CreditCardPaymentCommandHandler>("CreditCardPaymentCommandHandler");
            container.RegisterType<ICommandHandler, SmsCommandHandler>("SmsCommandHandler");
        }
    }
}