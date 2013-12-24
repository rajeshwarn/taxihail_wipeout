using System.Configuration;
using System.Data.Entity;
using Infrastructure;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using Infrastructure.Messaging.InMemory;
using Infrastructure.Serialization;
using Infrastructure.Sql.EventSourcing;
using Infrastructure.Sql.MessageLog;
using Microsoft.Practices.Unity;

namespace apcurium.MK.Web.SelfHost
{
    public class Module
    {
        public void Init(IUnityContainer container,ConnectionStringSettings connectionString)
        {
            RegisterInfrastructure(container, connectionString);

            new Common.Module().Init(container);
            new Booking.Module().Init(container);
            new Booking.Google.Module().Init(container);
            new Booking.Maps.Module().Init(container);
            new Booking.IBS.Module().Init(container);
            new Booking.Api.Module().Init(container);

            RegisterEventHandlers(container);
            RegisterCommandHandlers(container);
        }

        private void RegisterInfrastructure(IUnityContainer container, ConnectionStringSettings connectionString)
        {
            container.RegisterInstance(Common.Module.MkConnectionString, connectionString);
            Database.SetInitializer<EventStoreDbContext>(null);
            Database.SetInitializer<MessageLogDbContext>(null);


            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());

            // Event log database and handler.
            container.RegisterType<SqlMessageLog>(new InjectionConstructor(connectionString.ConnectionString, container.Resolve<ITextSerializer>(), container.Resolve<IMetadataProvider>()));
            container.RegisterType<IEventHandler, SqlMessageLogHandler>("SqlMessageLogHandler");
            container.RegisterType<ICommandHandler, SqlMessageLogHandler>("SqlMessageLogHandler");

            // Repository
            container.RegisterType<EventStoreDbContext>(new TransientLifetimeManager(), new InjectionConstructor(connectionString.ConnectionString));
            container.RegisterType(typeof(IEventSourcedRepository<>), typeof(SqlEventSourcedRepository<>), new ContainerControlledLifetimeManager());

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
