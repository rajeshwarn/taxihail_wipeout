#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Controllers;
using apcurium.MK.Booking.Services;
using Funq;
using Microsoft.Owin.Host.HttpListener;
using Microsoft.Owin.Hosting;
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

            var appHost = new AppHost();

            WebApp.Start(listeningOn, builder => appHost.Configure());

// ReSharper disable once LocalizableElement
            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);
            Console.ReadKey();
        }
    }
    
    public class AppHost
    {
        public AppHost()
        {
        }

        public void Init()
        {
            OwinServerFactory.Initialize(new Dictionary<string, object>());
            WebApp.Start("http://localhost:6901/api/", builder => Configure(builder));
        }

        private void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/v2/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "Legacy",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: null,
                handler: new LegacyHttpClientHandler()
            );
        }

        public void Configure(IAppBuilder builder)
        {
            new Module().Init(UnityServiceLocator.Instance, ConfigurationManager.ConnectionStrings["MKWebDev"]);

            var notificationService = UnityServiceLocator.Instance.Resolve<INotificationService>();
            notificationService.SetBaseUrl(new Uri("http://www.example.net"));

            builder.;
        }
    }
}