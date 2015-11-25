#region

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

#endregion

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
            container.RegisterType<IProjection<ServerPaymentSettings>, EntityProjection<ServerPaymentSettings>>(new ContainerControlledLifetimeManager(),
                new InjectionConstructor(typeof(Func<ConfigurationDbContext>), new object[] { AppConstants.CompanyId }));
            container.RegisterType<IProjectionSet<ServerPaymentSettings, string>, NetworkCompanyPaymentSettingsEntityProjections>(new ContainerControlledLifetimeManager());
        }

        public void RegisterEntityProjectionSets(UnityContainer container)
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
            container.RegisterType<IProjection<ServerPaymentSettings>, EntityProjection<ServerPaymentSettings>>(new ContainerControlledLifetimeManager(),
                new InjectionConstructor(typeof(Func<ConfigurationDbContext>), new object[] { AppConstants.CompanyId }));
            container.RegisterType<IProjectionSet<ServerPaymentSettings, string>, NetworkCompanyPaymentSettingsEntityProjections>(new ContainerControlledLifetimeManager());
        }

        private void RegisterInfrastructure(IUnityContainer container, ConnectionStringSettings connectionString)
        {
            container.RegisterInstance(apcurium.MK.Common.Module.MkConnectionString, connectionString);
            Database.SetInitializer<EventStoreDbContext>(null);
            Database.SetInitializer<MessageLogDbContext>(null);

            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());

            // Event log database and handler.
            container.RegisterType<SqlMessageLog>(new InjectionConstructor(connectionString.ConnectionString,
                container.Resolve<ITextSerializer>(), container.Resolve<IMetadataProvider>()));
            container.RegisterType<IEventHandler, SqlMessageLogHandler>("SqlMessageLogHandler");
            container.RegisterType<ICommandHandler, SqlMessageLogHandler>("SqlMessageLogHandler");

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