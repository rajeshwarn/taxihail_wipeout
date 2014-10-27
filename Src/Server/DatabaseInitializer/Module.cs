﻿#region

using System.Configuration;
using System.Data.Entity;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
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
        public void Init(IUnityContainer container, ConnectionStringSettings connectionString)
        {
            RegisterInfrastructure(container, connectionString);

            new apcurium.MK.Common.Module().Init(container);
            new apcurium.MK.Booking.Module().Init(container);
            new apcurium.MK.Booking.IBS.Module().Init(container);
            new apcurium.MK.Booking.Api.Module().Init(container);
            new apcurium.MK.Booking.MapDataProvider.Module().Init(container);
            new apcurium.MK.Booking.Maps.Module().Init(container);

            RegisterTaxiHailNetwork(container);
            RegisterEventHandlers(container);
            RegisterCommandHandlers(container);
            
            container.RegisterInstance<IEventsMigrator>(new EventsMigrator( () => container.Resolve<EventStoreDbContext>()));
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

            //ReplayEventService
            container.RegisterInstance<IEventsPlayBackService>(
                new EventsPlayBackService(() => container.Resolve<EventStoreDbContext>(), eventBus,
                    container.Resolve<ITextSerializer>()));
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
            var thNetworkServiceClient = new TaxiHailNetworkServiceClient(unityContainer.Resolve<IServerSettings>());
            unityContainer.RegisterInstance<ITaxiHailNetworkServiceClient>(thNetworkServiceClient);

        }

        private static void RegisterEventHandlers(IUnityContainer unityContainer)
        {
            var eventHandlerRegistry = unityContainer.Resolve<IEventHandlerRegistry>();
            // Filter out Integration Event Handlers
            // They should never replay events
            var eventHandlers = unityContainer.ResolveAll<IEventHandler>()
                .Where(x => !(x is IIntegrationEventHandler))
                .ToArray();

            foreach (var eventHandler in eventHandlers)
            {
                eventHandlerRegistry.Register(eventHandler);
            }
        }
    }
}