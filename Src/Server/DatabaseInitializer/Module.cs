using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Events.Migration;
using CustomerPortal.Client;
using CustomerPortal.Client.Impl;
using DatabaseInitializer.Services;
using Infrastructure;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using Infrastructure.Messaging.InMemory;
using Infrastructure.Serialization;
using Infrastructure.Sql.EventSourcing;
using Infrastructure.Sql.MessageLog;
using Microsoft.Practices.Unity;
using MK.Common.Configuration;

namespace DatabaseInitializer
{
    public class Module
    {
        public void Init(IUnityContainer container, ConnectionStringSettings connectionString, string oldConnectionString)
        {
            RegisterInfrastructure(container, connectionString);

            new apcurium.MK.Common.Module().Init(container);
            new apcurium.MK.Booking.Module().Init(container);
            new apcurium.MK.Booking.IBS.Module().Init(container);
            new apcurium.MK.Booking.Api.Module().Init(container);
            new apcurium.MK.Booking.MapDataProvider.Module().Init(container);
            new apcurium.MK.Booking.Maps.Module().Init(container);
            new apcurium.MK.Events.Migration.Module().Init(container);

            RegisterTaxiHailNetwork(container);
            RegisterEventHandlers(container);
            RegisterCommandHandlers(container);

            //ReplayEventService
            container.RegisterInstance<IEventsPlayBackService>(
                new EventsPlayBackService(() => container.Resolve<EventStoreDbContext>(),
                    container.Resolve<ITextSerializer>(), container.Resolve<EventMigrator>(), container.Resolve<ILogger>()));

            container.RegisterInstance<IEventsMigrator>(
                new EventsMigrator(() => container.Resolve<EventStoreDbContext>(),
                                   new ServerSettings(() => new ConfigurationDbContext(oldConnectionString), container.Resolve<ILogger>())));
        }

        public void RegisterMemoryProjectionSets(IUnityContainer container)
        {
            var accountDetailProjectionSet = new MemoryProjectionSet<AccountDetail>(a => a.Id);
            var orderDetailProjectionSet = new MemoryProjectionSet<OrderDetail>(a => a.Id);
            var orderStatusProjectionSet = new MemoryProjectionSet<OrderStatusDetail>(a => a.OrderId);
            var orderReportProjectionSet = new MemoryProjectionSet<OrderReportDetail>(a => a.Id);
            var orderPairingProjectionSet = new MemoryProjectionSet<OrderPairingDetail>(a => a.OrderId);
            var manualRideLinqProjectionSet = new MemoryProjectionSet<OrderManualRideLinqDetail>(a => a.OrderId);
            var orderNotificationProjectionSet = new MemoryProjectionSet<OrderNotificationDetail>(a => a.Id);
            var orderRatingProjectionSet = new OrderRatingMemoryProjectionSet();
            var addressDetailProjectionSet = new AddressDetailMemoryProjectionSet();
            var orderUserGpsProjectionSet = new MemoryProjectionSet<OrderUserGpsDetail>(a => a.OrderId);
            var vehicleTypeProjectionSet = new MemoryProjectionSet<VehicleTypeDetail>(a => a.Id);
            var orderPaymentProjectionSet = new MemoryProjectionSet<OrderPaymentDetail>(a => a.PaymentId);
            var notificationSettingsProjectionSet = new MemoryProjectionSet<NotificationSettings>(a => a.Id);
            var userTaxiHailNetworkSettingsProjectionSet = new MemoryProjectionSet<UserTaxiHailNetworkSettings>(a => a.Id);
            var tariffProjectionSet = new MemoryProjectionSet<TariffDetail>(a => a.Id);
            var companyProjectionSet = new MemoryProjectionSet<CompanyDetail>(a => a.Id);
            var feesProjectionSet = new MemoryProjectionSet<FeesDetail>(a => a.Id);
            var accountIbsDetailProjectionSet = new AccountIbsDetailMemoryProjectionSet();
            var promoProjectionSet = new MemoryProjectionSet<PromotionDetail>(a => a.Id);
            var promoUsageProjectionSet = new MemoryProjectionSet<PromotionUsageDetail>(a => a.OrderId);
            var promoProgressProjectionSet = new PromotionProgressDetailMemoryProjectionSet();
            var ruleProjectionSet = new MemoryProjectionSet<RuleDetail>(a => a.Id);
            var paypalAccountProjectionSet = new MemoryProjectionSet<PayPalAccountDetails>(a => a.AccountId);
            var creditCardProjectionSet = new MemoryProjectionSet<CreditCardDetails>(a => a.CreditCardId);
            var overduePaymentProjectionSet = new MemoryProjectionSet<OverduePaymentDetail>(a => a.OrderId);
            var ratingTypeProjectionSet = new RatingTypeDetailMemoryProjectionSet();

            container.RegisterInstance<IProjectionSet<AccountDetail>>(accountDetailProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderDetail>>(orderDetailProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderStatusDetail>>(orderStatusProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderReportDetail>>(orderReportProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderPairingDetail>>(orderPairingProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderManualRideLinqDetail>>(manualRideLinqProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderNotificationDetail>>(orderNotificationProjectionSet);
            container.RegisterInstance<AddressDetailProjectionSet>(addressDetailProjectionSet);
            container.RegisterInstance<AppSettingsProjection>(container.Resolve<AppSettingsEntityProjection>());
            container.RegisterInstance<OrderRatingProjectionSet>(orderRatingProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderUserGpsDetail>>(orderUserGpsProjectionSet);
            container.RegisterInstance<IProjectionSet<VehicleTypeDetail>>(vehicleTypeProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderPaymentDetail>>(orderPaymentProjectionSet);
            container.RegisterInstance<IProjectionSet<NotificationSettings>>(notificationSettingsProjectionSet);
            container.RegisterInstance<IProjectionSet<UserTaxiHailNetworkSettings>>(userTaxiHailNetworkSettingsProjectionSet);
            container.RegisterInstance<IProjectionSet<TariffDetail>>(tariffProjectionSet);
            container.RegisterInstance<IProjectionSet<CompanyDetail>>(companyProjectionSet);
            container.RegisterInstance<IProjectionSet<FeesDetail>>(feesProjectionSet);
            container.RegisterInstance<AccountIbsDetailProjectionSet>(accountIbsDetailProjectionSet);
            container.RegisterInstance<IProjectionSet<PromotionDetail>>(promoProjectionSet);
            container.RegisterInstance<IProjectionSet<PromotionUsageDetail>>(promoUsageProjectionSet);
            container.RegisterInstance<PromotionProgressDetailProjectionSet>(promoProgressProjectionSet);
            container.RegisterInstance<IProjectionSet<RuleDetail>>(ruleProjectionSet);
            container.RegisterInstance<IProjectionSet<PayPalAccountDetails>>(paypalAccountProjectionSet);
            container.RegisterInstance<IProjectionSet<CreditCardDetails>>(creditCardProjectionSet);
            container.RegisterInstance<IProjectionSet<OverduePaymentDetail>>(overduePaymentProjectionSet);
            container.RegisterInstance<RatingTypeDetailProjectionSet>(ratingTypeProjectionSet);
            
            container.RegisterType<IProjection<ServerPaymentSettings>, EntityProjection<ServerPaymentSettings>>(new ContainerControlledLifetimeManager(),
                new InjectionConstructor(typeof(Func<ConfigurationDbContext>), new object[] { AppConstants.CompanyId }));
            container.RegisterType<IProjectionSet<ServerPaymentSettings, string>, NetworkCompanyPaymentSettingsEntityProjections>(new ContainerControlledLifetimeManager());
        }

        public void RegisterEntityProjectionSets(IUnityContainer container)
        {
            var accountDetailProjectionSet = new EntityProjectionSet<AccountDetail>(container.Resolve<Func<BookingDbContext>>());
            var orderDetailProjectionSet = new EntityProjectionSet<OrderDetail>(container.Resolve<Func<BookingDbContext>>());
            var orderStatusProjectionSet = new EntityProjectionSet<OrderStatusDetail>(container.Resolve<Func<BookingDbContext>>());
            var orderReportProjectionSet = new EntityProjectionSet<OrderReportDetail>(container.Resolve<Func<BookingDbContext>>());
            var orderPairingProjectionSet = new EntityProjectionSet<OrderPairingDetail>(container.Resolve<Func<BookingDbContext>>());
            var manualRideLinqProjectionSet = new EntityProjectionSet<OrderManualRideLinqDetail>(container.Resolve<Func<BookingDbContext>>());
            var orderNotificationProjectionSet = new EntityProjectionSet<OrderNotificationDetail>(container.Resolve<Func<BookingDbContext>>());
            var orderRatingProjectionSet = new OrderRatingEntityProjectionSet(container.Resolve<Func<BookingDbContext>>());
            var addressDetailProjectionSet = new AddressDetailEntityProjectionSet(container.Resolve<Func<BookingDbContext>>());
            var orderUserGpsProjectionSet = new EntityProjectionSet<OrderUserGpsDetail>(container.Resolve<Func<BookingDbContext>>());
            var vehicleTypeProjectionSet = new EntityProjectionSet<VehicleTypeDetail>(container.Resolve<Func<BookingDbContext>>());
            var orderPaymentProjectionSet = new EntityProjectionSet<OrderPaymentDetail>(container.Resolve<Func<BookingDbContext>>());
            var notificationSettingsProjectionSet = new EntityProjectionSet<NotificationSettings>(container.Resolve<Func<ConfigurationDbContext>>());
            var userTaxiHailNetworkSettingsProjectionSet = new EntityProjectionSet<UserTaxiHailNetworkSettings>(container.Resolve<Func<ConfigurationDbContext>>());
            var tariffProjectionSet = new EntityProjectionSet<TariffDetail>(container.Resolve<Func<BookingDbContext>>());
            var companyProjectionSet = new EntityProjectionSet<CompanyDetail>(container.Resolve<Func<BookingDbContext>>());
            var feesProjectionSet = new EntityProjectionSet<FeesDetail>(container.Resolve<Func<BookingDbContext>>());
            var accountIbsDetailProjectionSet = new AccountIbsDetailEntityProjectionSet(container.Resolve<Func<BookingDbContext>>());
            var promoProjectionSet = new EntityProjectionSet<PromotionDetail>(container.Resolve<Func<BookingDbContext>>());
            var promoUsageProjectionSet = new EntityProjectionSet<PromotionUsageDetail>(container.Resolve<Func<BookingDbContext>>());
            var promoProgressProjectionSet = new PromotionProgressDetailEntityProjectionSet(container.Resolve<Func<BookingDbContext>>());
            var ruleProjectionSet = new EntityProjectionSet<RuleDetail>(container.Resolve<Func<BookingDbContext>>());
            var paypalAccountProjectionSet = new EntityProjectionSet<PayPalAccountDetails>(container.Resolve<Func<BookingDbContext>>());
            var creditCardProjectionSet = new EntityProjectionSet<CreditCardDetails>(container.Resolve<Func<BookingDbContext>>());
            var overduePaymentProjectionSet = new EntityProjectionSet<OverduePaymentDetail>(container.Resolve<Func<BookingDbContext>>());
            var ratingTypeProjectionSet = new RatingTypeDetailEntityProjectionSet(container.Resolve<Func<BookingDbContext>>());

            container.RegisterInstance<IProjectionSet<AccountDetail>>(accountDetailProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderDetail>>(orderDetailProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderStatusDetail>>(orderStatusProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderReportDetail>>(orderReportProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderPairingDetail>>(orderPairingProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderManualRideLinqDetail>>(manualRideLinqProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderNotificationDetail>>(orderNotificationProjectionSet);
            container.RegisterInstance<AddressDetailProjectionSet>(addressDetailProjectionSet);
            container.RegisterInstance<AppSettingsProjection>(container.Resolve<AppSettingsEntityProjection>());
            container.RegisterInstance<OrderRatingProjectionSet>(orderRatingProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderUserGpsDetail>>(orderUserGpsProjectionSet);
            container.RegisterInstance<IProjectionSet<VehicleTypeDetail>>(vehicleTypeProjectionSet);
            container.RegisterInstance<IProjectionSet<OrderPaymentDetail>>(orderPaymentProjectionSet);
            container.RegisterInstance<IProjectionSet<NotificationSettings>>(notificationSettingsProjectionSet);
            container.RegisterInstance<IProjectionSet<UserTaxiHailNetworkSettings>>(userTaxiHailNetworkSettingsProjectionSet);
            container.RegisterInstance<IProjectionSet<TariffDetail>>(tariffProjectionSet);
            container.RegisterInstance<IProjectionSet<CompanyDetail>>(companyProjectionSet);
            container.RegisterInstance<IProjectionSet<FeesDetail>>(feesProjectionSet);
            container.RegisterInstance<AccountIbsDetailProjectionSet>(accountIbsDetailProjectionSet);
            container.RegisterInstance<IProjectionSet<PromotionDetail>>(promoProjectionSet);
            container.RegisterInstance<IProjectionSet<PromotionUsageDetail>>(promoUsageProjectionSet);
            container.RegisterInstance<PromotionProgressDetailProjectionSet>(promoProgressProjectionSet);
            container.RegisterInstance<IProjectionSet<RuleDetail>>(ruleProjectionSet);
            container.RegisterInstance<IProjectionSet<PayPalAccountDetails>>(paypalAccountProjectionSet);
            container.RegisterInstance<IProjectionSet<CreditCardDetails>>(creditCardProjectionSet);
            container.RegisterInstance<IProjectionSet<OverduePaymentDetail>>(overduePaymentProjectionSet);
            container.RegisterInstance<RatingTypeDetailProjectionSet>(ratingTypeProjectionSet);

            container.RegisterType<IProjection<ServerPaymentSettings>, EntityProjection<ServerPaymentSettings>>(new ContainerControlledLifetimeManager(),
                new InjectionConstructor(typeof(Func<ConfigurationDbContext>), new object[] { AppConstants.CompanyId }));
            container.RegisterType<IProjectionSet<ServerPaymentSettings, string>, NetworkCompanyPaymentSettingsEntityProjections>(new ContainerControlledLifetimeManager());
        }

        private void RegisterInfrastructure(IUnityContainer container, ConnectionStringSettings connectionString)
        {
            container.RegisterInstance(apcurium.MK.Common.Module.MkConnectionString, connectionString);
            Database.SetInitializer<EventStoreDbContext>(null);

            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());

            // Repository
            container.RegisterType<EventStoreDbContext>(new TransientLifetimeManager(),
                new InjectionConstructor(connectionString.ConnectionString));
            container.RegisterType(typeof (IEventSourcedRepository<>), typeof (SqlEventSourcedRepository<>),
                new ContainerControlledLifetimeManager());

            // Command bus
            var commandBus = new SynchronousMemoryCommandBus();
            container.RegisterInstance<ICommandBus>(commandBus);
            container.RegisterInstance<ICommandHandlerRegistry>(commandBus);

            // Event bus
            var eventBus = new SynchronousMemoryEventBus();
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<IEventHandlerRegistry>(eventBus);
        }

        private static void RegisterCommandHandlers(IUnityContainer unityContainer)
        {
            var commandHandlerRegistry = unityContainer.Resolve<ICommandHandlerRegistry>();

            foreach (var commandHandler in unityContainer.ResolveAll<ICommandHandler>())
            {
                commandHandlerRegistry.Register(commandHandler);
            }
        }

        private static void RegisterTaxiHailNetwork(IUnityContainer unityContainer)
        {
            unityContainer.RegisterType<ITaxiHailNetworkServiceClient>(
                new TransientLifetimeManager(),
                new InjectionFactory(c => new TaxiHailNetworkServiceClient(c.Resolve<IServerSettings>())));
        }

        private static void RegisterEventHandlers(IUnityContainer unityContainer)
        {
            var eventHandlerRegistry = unityContainer.Resolve<IEventHandlerRegistry>();

            // Filter out Integration Event Handlers
            // They should never replay events
            var eventHandlerRegistrations = unityContainer.Registrations
                .Where(x => x.RegisteredType == typeof(IEventHandler)
                        && !x.MappedToType.GetInterfaces().Contains(typeof(IIntegrationEventHandler)))
                .ToArray();

            foreach (var eventHandlerRegistration in eventHandlerRegistrations)
            {
                var eventHandler = (IEventHandler)unityContainer.Resolve(eventHandlerRegistration.MappedToType);
                eventHandlerRegistry.Register(eventHandler);
            }

            //var eventHandler = (IEventHandler)unityContainer.Resolve(typeof(AccountDetailsGenerator));
            //eventHandlerRegistry.Register(eventHandler);
        }
    }
}