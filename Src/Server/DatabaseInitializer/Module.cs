﻿#region

using System.Configuration;
using System.Data.Entity;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Services;
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
using PCLCrypto;

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

            RegisterTaxiHailNetwork(container);
            RegisterEventHandlers(container);
            RegisterCommandHandlers(container);
            
            container.RegisterInstance<IEventsMigrator>(
                new EventsMigrator(() => container.Resolve<EventStoreDbContext>(),
                                   new ServerSettings(() => new ConfigurationDbContext(oldConnectionString), container.Resolve<ILogger>())));
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
                var eventHandler = (IEventHandler) unityContainer.Resolve(eventHandlerRegistration.MappedToType);
                eventHandlerRegistry.Register(eventHandler);
            }
        }
    }
}