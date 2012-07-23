using System;
using Infrastructure;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using Infrastructure.Messaging.InMemory;
using Infrastructure.Sql.BlobStorage;
using Infrastructure.Sql.EventSourcing;
using Infrastructure.Sql.MessageLog;
using Microsoft.Practices.Unity;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.WebHost.Endpoints;
using Funq;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Validation;
using apcurium.MK.Booking.BackOffice.CommandHandlers;
using apcurium.MK.Booking.BackOffice.EventHandlers;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Email;
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
using apcurium.MK.Common.IoC;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var listeningOn = args.Length == 0 ? "http://*:6901/" : args[0];

            var appHost = new AppHost();
            appHost.Init();
            appHost.Start(listeningOn);

            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);
            Console.ReadKey();
        }


    }

    public class AppHost : AppHostHttpListenerBase
    {

        public AppHost() : base("Mobile Knowledge Web Services", typeof(CurrentAccountService).Assembly) { }

        public override void Configure(Container containerFunq)
        {
            new Module().Init(UnityServiceLocator.Instance);

            var container = UnityServiceLocator.Instance;
            containerFunq.Adapter = new UnityContainerAdapter(container, new Logger());


            Plugins.Add(new AuthFeature(() => new AuthUserSession(), new IAuthProvider[] { new CustomCredentialsAuthProvider(container.Resolve<IAccountDao>(), container.Resolve<IPasswordService>()) }));
            Plugins.Add(new ValidationFeature());
            containerFunq.RegisterValidators(typeof(SaveFavoriteAddressValidator).Assembly);

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