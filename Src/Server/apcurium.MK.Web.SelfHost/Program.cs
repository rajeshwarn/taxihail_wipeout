using System;
using System.Configuration;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.WebHost.Endpoints;
using Funq;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Validation;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Security;
using apcurium.MK.Common.IoC;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var listeningOn = args.Length == 0 ? "http://*:6901/api/" : args[0];

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
            new Module().Init(UnityServiceLocator.Instance, ConfigurationManager.ConnectionStrings["MKWebDev"]);

            var container = UnityServiceLocator.Instance;
            containerFunq.Adapter = new UnityContainerAdapter(container, new Logger());

            Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                new IAuthProvider[]
                    {
                        new CustomCredentialsAuthProvider(container.Resolve<ICommandBus>(), container.Resolve<IAccountDao>(), container.Resolve<IPasswordService>()),
                        new CustomFacebookAuthProvider(container.Resolve<IAccountDao>()), 
                        new CustomTwitterAuthProvider(container.Resolve<IAccountDao>()), 
                    }));
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