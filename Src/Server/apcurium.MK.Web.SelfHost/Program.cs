#region

using System;
using System.Configuration;
using apcurium.MK.Booking.Api.Security;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Validation;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.IoC;
using Funq;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.WebHost.Endpoints;
using UnityContainerExtensions = Microsoft.Practices.Unity.UnityContainerExtensions;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

#endregion

namespace apcurium.MK.Web.SelfHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var listeningOn = args.Length == 0 ? "http://*:6901/api/" : args[0];

            var appHost = new AppHost();
            appHost.Init();
            appHost.Start(listeningOn);

// ReSharper disable once LocalizableElement
            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);
            Console.ReadKey();
        }
    }

    public class AppHost : AppHostHttpListenerBase
    {
        public AppHost() : base("Mobile Knowledge Web Services", typeof (CurrentAccountService).Assembly)
        {
        }

        public override void Configure(Container containerFunq)
        {
            var notificationService = UnityContainerExtensions.Resolve<INotificationService>(UnityServiceLocator.Instance);
            notificationService.SetBaseUrl(new Uri("http://www.example.com"));
            
            new Module().Init(UnityServiceLocator.Instance, ConfigurationManager.ConnectionStrings["MKWebDev"]);

            var container = UnityServiceLocator.Instance;
            containerFunq.Adapter = new UnityContainerAdapter(container, new Logger());

            Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                new IAuthProvider[]
                {
                    new CustomCredentialsAuthProvider(UnityContainerExtensions.Resolve<ICommandBus>(container),
                        UnityContainerExtensions.Resolve<IAccountDao>(container),
                        UnityContainerExtensions.Resolve<IPasswordService>(container)),
                    new CustomFacebookAuthProvider(UnityContainerExtensions.Resolve<IAccountDao>(container)),
                    new CustomTwitterAuthProvider(UnityContainerExtensions.Resolve<IAccountDao>(container))
                }));
            Plugins.Add(new ValidationFeature());
            containerFunq.RegisterValidators(typeof (SaveFavoriteAddressValidator).Assembly);

            

            SetConfig(new EndpointHostConfig
            {
                GlobalResponseHeaders =
                {
                    {"Access-Control-Allow-Origin", "*"},
                    {"Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS"},
                },
            });
        }
    }
}