using System;
using Infrastructure;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using Infrastructure.Messaging.InMemory;
using Infrastructure.Sql.BlobStorage;
using Infrastructure.Sql.EventSourcing;
using Infrastructure.Sql.MessageLog;
using Microsoft.Practices.Unity;
using ServiceStack.WebHost.Endpoints;
using Funq;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.BackOffice.CommandHandlers;
using apcurium.MK.Booking.BackOffice.EventHandlers;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Database;
using Infrastructure.Messaging;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using Infrastructure.Serialization;
using System.Data.Entity;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Security;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using apcurium.MK.Common.Entity;
using apcurium.MK.Web.SelfHost.IoC;

namespace apcurium.MK.Web.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var listeningOn = args.Length == 0 ? "http://*:6901/" : args[0];

            Database.DefaultConnectionFactory = new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory);
            
            var appHost = new AppHost();
            appHost.Init();
            appHost.Start(listeningOn);

            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);
            Console.ReadKey();
        }

        public class AppHost : AppHostHttpListenerBase
        {

            public AppHost() : base("Mobile Knowledge Web Services", typeof(CurrentAccountService).Assembly) { }

            public override void Configure(Container containerFunq)
            {
                Database.SetInitializer<BookingDbContext>(null);
                Database.SetInitializer<ConfigurationDbContext>(null);
                Database.SetInitializer<EventStoreDbContext>(null);
                Database.SetInitializer<MessageLogDbContext>(null);
                Database.SetInitializer<BlobStorageDbContext>(null);

                containerFunq.Adapter = new UnityContainerAdapter(IoC.UnityServiceLocator.Instance, new Logger());

                var container = IoC.UnityServiceLocator.Instance;

                container.RegisterType<BookingDbContext>(new TransientLifetimeManager(), new InjectionConstructor("MKWeb"));
                container.RegisterType<ConfigurationDbContext>(new TransientLifetimeManager(), new InjectionConstructor("MKWeb"));
                container.RegisterInstance<IAccountDao>(new AccountDao(() => container.Resolve<BookingDbContext>()));
                container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());
                container.RegisterInstance<IMetadataProvider>(new StandardMetadataProvider());


                container.RegisterInstance<IConfigurationManager>(new Common.Configuration.Impl.ConfigurationManager(() => container.Resolve<ConfigurationDbContext>()));
                container.RegisterInstance<IAccountWebServiceClient>(new AccountWebServiceClient(container.Resolve<IConfigurationManager>(), new Logger()));

                // Event log database and handler.
                container.RegisterType<SqlMessageLog>(new InjectionConstructor("MessageLog", container.Resolve<ITextSerializer>(), container.Resolve<IMetadataProvider>()));
                container.RegisterType<IEventHandler, SqlMessageLogHandler>("SqlMessageLogHandler");
                container.RegisterType<ICommandHandler, SqlMessageLogHandler>("SqlMessageLogHandler");


                container.RegisterInstance<IEventBus>(new MemoryEventBus(container.Resolve<AccountDetailsGenerator>(), container.Resolve<FavoriteAddressListGenerator>(), container.Resolve<SqlMessageLogHandler>()));

                container.RegisterType<EventStoreDbContext>(new TransientLifetimeManager(), new InjectionConstructor("EventStore"));
                container.RegisterType(typeof(IEventSourcedRepository<>), typeof(SqlEventSourcedRepository<>), new ContainerControlledLifetimeManager());

                container.RegisterInstance<IPasswordService>(new PasswordService());
                container.RegisterType<ICommandHandler, AccountCommandHandler>("AccountCommandHandler");
                container.RegisterType<ICommandHandler, FavoriteAddressCommandHandler>("FavoriteAddressCommandHandler");
                container.RegisterInstance<ICommandBus>(new MemoryCommandBus(container.Resolve<ICommandHandler>("AccountCommandHandler"), container.Resolve<ICommandHandler>("FavoriteAddressCommandHandler")));


                Plugins.Add(new AuthFeature(() => new AuthUserSession(), new IAuthProvider[] { new CustomCredentialsAuthProvider(container.Resolve<IAccountDao>(), container.Resolve<IPasswordService>()) }));

                container.RegisterInstance<ICacheClient>(new MemoryCacheClient { FlushOnDispose = false });

                SetConfig(new EndpointHostConfig
                {
                    GlobalResponseHeaders =
                        {
                            { "Access-Control-Allow-Origin", "*" },
                            { "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
                        },
                });

            }
        }
    }

    
}
