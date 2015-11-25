#region

using System;
using System.Configuration;
using System.Data.Entity;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using CustomerPortal.Client;
using CustomerPortal.Client.Impl;
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

namespace apcurium.MK.Web.SelfHost
{
    public class Module
    {
        public void Init(IUnityContainer container, ConnectionStringSettings connectionString)
        {
            RegisterInfrastructure(container, connectionString);

            RegisterEntityProjectionSets(container);

            new Common.Module().Init(container);
            new Booking.Module().Init(container);
            new Booking.MapDataProvider.Module().Init(container);
            new Booking.Maps.Module().Init(container);
            new Booking.IBS.Module().Init(container);
            new Booking.Api.Module().Init(container);

            RegisterTaxiHailNetwork(container);
            RegisterEventHandlers(container);
            RegisterCommandHandlers(container);
        }

        private void RegisterEntityProjectionSets(IUnityContainer container)
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

        private static void RegisterTaxiHailNetwork(IUnityContainer unityContainer)
        {
            var thNetworkServiceClient = new TaxiHailNetworkServiceClient(unityContainer.Resolve<IServerSettings>());
            unityContainer.RegisterInstance<ITaxiHailNetworkServiceClient>(thNetworkServiceClient);

        }

        private void RegisterInfrastructure(IUnityContainer container, ConnectionStringSettings connectionString)
        {
            container.RegisterInstance(Common.Module.MkConnectionString, connectionString);
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

        private static void RegisterEventHandlers(IUnityContainer unityContainer)
        {
            var commandHandlerRegistry = unityContainer.Resolve<IEventHandlerRegistry>();

            foreach (var eventHandler in unityContainer.ResolveAll<IEventHandler>())
            {
                commandHandlerRegistry.Register(eventHandler);
            }
        }
    }
}