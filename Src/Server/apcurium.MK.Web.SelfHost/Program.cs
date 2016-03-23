#region

using System;
using System.Configuration;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Controllers;
using apcurium.MK.Booking.Api.Controllers;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.IoC;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Practices.Unity;
using Owin;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

#endregion

namespace apcurium.MK.Web.SelfHost
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var listeningOn = args.Length == 0 ? "http://*:6901/api/" : args[0];

            WebApp.Start<AppHost>(new StartOptions(listeningOn)
            {
                ServerFactory = "Microsoft.Owin.Host.HttpListener"
            });

            // ReSharper disable once LocalizableElement
            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);
            Console.ReadKey();
        }
    }
    
    public class AppHost
    {

        private HttpConfiguration MapRoutes(HttpConfiguration config, IUnityContainer container)
        {
            config.MapHttpAttributeRoutes();

            config.MessageHandlers.Add(new TaxihailApiHttpHandler());      
            
            config.DependencyResolver = new UnityContainerAdapter(container, container.Resolve<ILogger>());

            config.Filters.Add(new ValidationFilterAttribute());

            return config;
        }

        public void Configuration(IAppBuilder builder)
        {
            var container = UnityServiceLocator.Instance;

            new Module().Init(container, ConfigurationManager.ConnectionStrings["MKWebDev"]);

            var notificationService = UnityServiceLocator.Instance.Resolve<INotificationService>();
            notificationService.SetBaseUrl(new Uri("http://www.example.net"));

            builder.UseWebApi(MapRoutes(new HttpConfiguration(), container))
               .UseCookieAuthentication(new CookieAuthenticationOptions());
            
        }
    }
}