using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Infrastructure;
using Infrastructure.BlobStorage;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using Infrastructure.Serialization;
using Infrastructure.Sql.BlobStorage;
using Infrastructure.Sql.EventSourcing;
using Infrastructure.Sql.MessageLog;
using Infrastructure.Sql.Messaging;
using Infrastructure.Sql.Messaging.Handling;
using Infrastructure.Sql.Messaging.Implementation;
using Microsoft.Practices.Unity;
using apcurium.MK.Booking.BackOffice.EventHandlers;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;

namespace WorkerRoleCommandProcessor
{
    public class Module
    {
        public void Init(IUnityContainer container, string databaseName)
        {
            Database.DefaultConnectionFactory = new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory);
            Database.SetInitializer<EventStoreDbContext>(null);
            Database.SetInitializer<MessageLogDbContext>(null);
            Database.SetInitializer<BlobStorageDbContext>(null);
            Database.SetInitializer<BookingDbContext>(null);
            Database.SetInitializer<ConfigurationDbContext>(null);

            container.RegisterType<BookingDbContext>(new TransientLifetimeManager(), new InjectionConstructor(databaseName));
            container.RegisterType<ConfigurationDbContext>(new TransientLifetimeManager(), new InjectionConstructor(databaseName));

            // Infrastructure
            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());
            container.RegisterType<IBlobStorage, SqlBlobStorage>(new ContainerControlledLifetimeManager(), new InjectionConstructor("BlobStorage"));

            // Infrastructure -  Event log database and handler.
            container.RegisterType<SqlMessageLog>(new InjectionConstructor("MessageLog", container.Resolve<ITextSerializer>(), container.Resolve<IMetadataProvider>()));
            container.RegisterType<IEventHandler, SqlMessageLogHandler>("SqlMessageLogHandler");
            container.RegisterType<ICommandHandler, SqlMessageLogHandler>("SqlMessageLogHandler");
            
            // Common
            container.RegisterType<ILogger, Logger>();
            container.RegisterInstance<IConfigurationManager>(new ConfigurationManager(() => container.Resolve<ConfigurationDbContext>()));
            

            new apcurium.MK.Booking.Module().Init(container);
            new apcurium.MK.Booking.IBS.Module().Init(container);

            RegisterRepository(container);
            RegisterCommandBus(container, databaseName);
            RegisterEventBus(container, databaseName);
        }

        private void RegisterCommandBus(IUnityContainer container, string databaseName)
        {
            var commandBus = new CommandBus(new MessageSender(Database.DefaultConnectionFactory, databaseName, "SqlBus.Commands"), container.Resolve<ITextSerializer>());
            var commandProcessor = new CommandProcessor(new MessageReceiver(Database.DefaultConnectionFactory, databaseName, "SqlBus.Commands"), container.Resolve<ITextSerializer>());
            container.RegisterInstance<ICommandBus>(commandBus);
            container.RegisterInstance<ICommandHandlerRegistry>(commandProcessor);
            container.RegisterInstance<IProcessor>("CommandProcessor", commandProcessor);

            RegisterCommandHandlers(container);
        }

        private void RegisterEventBus(IUnityContainer container, string databaseName)
        {
            var eventBus = new EventBus(new MessageSender(Database.DefaultConnectionFactory, databaseName, "SqlBus.Events"), container.Resolve<ITextSerializer>());
            var eventProcessor = new EventProcessor(new MessageReceiver(Database.DefaultConnectionFactory, databaseName, "SqlBus.Events"), container.Resolve<ITextSerializer>());
            new apcurium.MK.Booking.Module().RegisterEventHandlers(container, eventProcessor);
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<IEventHandlerRegistry>(eventProcessor);
            container.RegisterInstance<IProcessor>("EventProcessor", eventProcessor);

            eventProcessor.Register(container.Resolve<SqlMessageLogHandler>());
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
