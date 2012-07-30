using System.Data.Entity;
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
using apcurium.MK.Booking.Database;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;

namespace WorkerRoleCommandProcessor
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            RegisterInfrastructure(container);
            
            new apcurium.MK.Common.Module().Init(container);
            new apcurium.MK.Booking.Module().Init(container);
            new apcurium.MK.Booking.IBS.Module().Init(container);

            RegisterEventHandlers(container);
            RegisterCommandHandlers(container);
        }

        private void RegisterInfrastructure(IUnityContainer container)
        {
            Database.DefaultConnectionFactory = new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory);
            Database.SetInitializer<EventStoreDbContext>(null);
            Database.SetInitializer<MessageLogDbContext>(null);
            Database.SetInitializer<BlobStorageDbContext>(null);

            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());
            container.RegisterType<IBlobStorage, SqlBlobStorage>(new ContainerControlledLifetimeManager(), new InjectionConstructor("BlobStorage"));

            // Event log database and handler.
            container.RegisterType<SqlMessageLog>(new InjectionConstructor("MessageLog", container.Resolve<ITextSerializer>(), container.Resolve<IMetadataProvider>()));
            container.RegisterType<IEventHandler, SqlMessageLogHandler>("SqlMessageLogHandler");
            container.RegisterType<ICommandHandler, SqlMessageLogHandler>("SqlMessageLogHandler");

            // Repository
            container.RegisterType<EventStoreDbContext>(new TransientLifetimeManager(), new InjectionConstructor("EventStore"));
            container.RegisterType(typeof(IEventSourcedRepository<>), typeof(SqlEventSourcedRepository<>), new ContainerControlledLifetimeManager());

            // Command bus
            var commandBus = new CommandBus(new MessageSender(Database.DefaultConnectionFactory, "SqlBus", "SqlBus.Commands"), container.Resolve<ITextSerializer>());
            var commandProcessor = new CommandProcessor(new MessageReceiver(Database.DefaultConnectionFactory, "SqlBus", "SqlBus.Commands"), container.Resolve<ITextSerializer>());
            container.RegisterInstance<ICommandBus>(commandBus);
            container.RegisterInstance<ICommandHandlerRegistry>(commandProcessor);
            container.RegisterInstance<IProcessor>("CommandProcessor", commandProcessor);

            // Event Bus
            var eventBus = new EventBus(new MessageSender(Database.DefaultConnectionFactory, "SqlBus", "SqlBus.Events"), container.Resolve<ITextSerializer>());
            var eventProcessor = new EventProcessor(new MessageReceiver(Database.DefaultConnectionFactory, "SqlBus", "SqlBus.Events"), container.Resolve<ITextSerializer>());
            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<IEventHandlerRegistry>(eventProcessor);
            container.RegisterInstance<IProcessor>("EventProcessor", eventProcessor);
            
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
