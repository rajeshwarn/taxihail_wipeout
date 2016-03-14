#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using apcurium.MK.Booking.Api.Controllers;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.IoC;
using Funq;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Host.HttpListener;
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
        private class DirectRouteResolver : DefaultDirectRouteProvider
        {
            private readonly Func<ILogger> _getLogger = () => UnityServiceLocator.Instance.Resolve<ILogger>();

            protected override IReadOnlyList<RouteEntry> GetActionDirectRoutes(HttpActionDescriptor actionDescriptor, IReadOnlyList<IDirectRouteFactory> factories,
                IInlineConstraintResolver constraintResolver)
            {
                try
                {
                    return base.GetActionDirectRoutes(actionDescriptor, factories, constraintResolver);
                }
                catch (Exception ex)
                {
                    _getLogger().LogError(ex);
                    throw;
                }
            }
        }

        public AppHost()
        {
        }
        private HttpConfiguration MapRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes(new DirectRouteResolver());

            config.MessageHandlers.Add(new LegacyHttpClientHandler());

            config.DependencyResolver = new UnityContainerAdapter(UnityServiceLocator.Instance, UnityServiceLocator.Instance.Resolve<ILogger>());

            return config;
        }

        public void Configuration(IAppBuilder builder)
        {
            new Module().Init(UnityServiceLocator.Instance, ConfigurationManager.ConnectionStrings["MKWebDev"]);

            var notificationService = UnityServiceLocator.Instance.Resolve<INotificationService>();
            notificationService.SetBaseUrl(new Uri("http://www.example.net"));

            builder.UseWebApi(MapRoutes(new HttpConfiguration()))
               .UseCookieAuthentication(new CookieAuthenticationOptions());
        }
    }
}