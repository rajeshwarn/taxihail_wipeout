using System;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using Infrastructure;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using Infrastructure.Messaging.InMemory;
using Infrastructure.Serialization;
using Infrastructure.Sql.EventSourcing;
using Infrastructure.Sql.MessageLog;
using Microsoft.Practices.Unity;
using PushSharp.Android;
using PushSharp.Apple;
using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.PushNotifications.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Web
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            RegisterInfrastructure(container);

            new MK.Common.Module().Init(container);
            new MK.Booking.Module().Init(container);
            new MK.Booking.Google.Module().Init(container);
            new MK.Booking.Maps.Module().Init(container);
            new MK.Booking.IBS.Module().Init(container);
            new MK.Booking.Api.Module().Init(container);

            RegisterEventHandlers(container);
            RegisterCommandHandlers(container);
        }

        private void RegisterInfrastructure(IUnityContainer container)
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["MKWeb"];
            container.RegisterInstance(apcurium.MK.Common.Module.MkConnectionString, connectionStringSettings);

            Database.SetInitializer<EventStoreDbContext>(null);
            Database.SetInitializer<MessageLogDbContext>(null);

            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
            container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());

            // Event log database and handler.
            container.RegisterType<SqlMessageLog>(new InjectionConstructor(connectionStringSettings.ConnectionString, container.Resolve<ITextSerializer>(), container.Resolve<IMetadataProvider>()));
            container.RegisterType<IEventHandler, SqlMessageLogHandler>("SqlMessageLogHandler");
            container.RegisterType<ICommandHandler, SqlMessageLogHandler>("SqlMessageLogHandler");

            // Repository
            container.RegisterType<EventStoreDbContext>(new TransientLifetimeManager(), new InjectionConstructor(connectionStringSettings.ConnectionString));
            container.RegisterType(typeof(IEventSourcedRepository<>), typeof(SqlEventSourcedRepository<>), new ContainerControlledLifetimeManager());

            // Command bus
            var commandBus = new AsynchronousMemoryCommandBus(container.Resolve<ITextSerializer>());
            container.RegisterInstance<ICommandBus>(commandBus);
            container.RegisterInstance<ICommandHandlerRegistry>(commandBus);

            // Event bus
            var eventBus = new AsynchronousMemoryEventBus(container.Resolve<ITextSerializer>());
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
            var eventHandlerRegistry = unityContainer.Resolve<IEventHandlerRegistry>();

            foreach (var eventHandler in unityContainer.ResolveAll<IEventHandler>())
            {
                eventHandlerRegistry.Register(eventHandler);
            }
        }
    }
}