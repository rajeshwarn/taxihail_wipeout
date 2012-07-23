using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Infrastructure;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using Infrastructure.Messaging.InMemory;
using Infrastructure.Serialization;
using Infrastructure.Sql.BlobStorage;
using Infrastructure.Sql.EventSourcing;
using Infrastructure.Sql.MessageLog;
using Infrastructure.Sql.Messaging;
using Infrastructure.Sql.Messaging.Handling;
using Infrastructure.Sql.Messaging.Implementation;
using Microsoft.Practices.Unity;
using apcurium.MK.Booking.BackOffice.CommandHandlers;
using apcurium.MK.Booking.BackOffice.EventHandlers;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace apcurium.MK.Web.SelfHost
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            Database.DefaultConnectionFactory = new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory);
            Database.SetInitializer<BookingDbContext>(null);
            Database.SetInitializer<ConfigurationDbContext>(null);
            Database.SetInitializer<EventStoreDbContext>(null);
            Database.SetInitializer<MessageLogDbContext>(null);
            Database.SetInitializer<BlobStorageDbContext>(null); 

            container.RegisterType<BookingDbContext>(new TransientLifetimeManager(), new InjectionConstructor("MKWeb"));
            container.RegisterType<ConfigurationDbContext>(new TransientLifetimeManager(), new InjectionConstructor("MKWeb"));

            // Infrastructure
            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());

            // Infrastructure - Event log database and handler.
            container.RegisterType<SqlMessageLog>(new InjectionConstructor("MessageLog", container.Resolve<ITextSerializer>(), container.Resolve<IMetadataProvider>()));
            container.RegisterType<IEventHandler, SqlMessageLogHandler>("SqlMessageLogHandler");
            container.RegisterType<ICommandHandler, SqlMessageLogHandler>("SqlMessageLogHandler");

            // Common
            container.RegisterInstance<ILogger>(new Logger());
            container.RegisterInstance<IConfigurationManager>(new Common.Configuration.Impl.ConfigurationManager(() => container.Resolve<ConfigurationDbContext>()));

            new MK.Booking.Module().Init(container);
            new MK.Booking.IBS.Module().Init(container);
            new MK.Booking.Api.Module().Init(container);

            RegisterRepository(container);
            RegisterEventBus(container);
            RegisterCommandBus(container);

        }

        private void RegisterCommandBus(IUnityContainer container)
        {
            var commandBus = new MemoryCommandBus();
            container.RegisterInstance<ICommandBus>(commandBus);
            container.RegisterInstance<ICommandHandlerRegistry>(commandBus);

            RegisterCommandHandlers(container);
        }

        private void RegisterEventBus(IUnityContainer container)
        {
            var eventBus = new MemoryEventBus();
            new MK.Booking.Module().RegisterEventHandlers(container, eventBus);
            container.RegisterInstance<IEventBus>(eventBus);

            eventBus.Register(container.Resolve<SqlMessageLogHandler>());            
        }

        private void RegisterRepository(IUnityContainer container)
        {
            // repository
            container.RegisterType<EventStoreDbContext>(new TransientLifetimeManager(), new InjectionConstructor("EventStore"));
            container.RegisterType(typeof(IEventSourcedRepository<>), typeof(SqlEventSourcedRepository<>), new ContainerControlledLifetimeManager());
        }

        private static void RegisterCommandHandlers(IUnityContainer unityContainer)
        {
            var commandHandlerRegistry = unityContainer.Resolve<ICommandHandlerRegistry>();

            foreach (var commandHandler in unityContainer.ResolveAll<ICommandHandler>())
            {
                commandHandlerRegistry.Register(commandHandler);
            }
        }
    }
}
