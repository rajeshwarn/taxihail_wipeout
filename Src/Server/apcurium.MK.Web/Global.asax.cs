using System;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using System.Web;
using System.Web.Optimization;
using log4net;
using Microsoft.Practices.Unity;
using apcurium.MK.Booking.Api.Jobs;
using apcurium.MK.Common.Configuration;
using log4net.Config;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web
{
    public class Global : HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Global));      
        

        protected void Application_Start(object sender, EventArgs e)
        {

            XmlConfigurator.Configure();
            new MkWebAppHost().Init();
            
            var config = UnityServiceLocator.Instance.Resolve<IConfigurationManager>();
            BundleConfig.RegisterBundles(BundleTable.Bundles, config.GetSetting("TaxiHail.ApplicationKey"));

            
            StatusJobService = UnityServiceLocator.Instance.Resolve<IUpdateOrderStatusJob>();

            var configurationManager = UnityServiceLocator.Instance.Resolve<IConfigurationManager>();
            int pollingValue;
            if (!int.TryParse(configurationManager.GetSetting("OrderStatus.ServerPollingInterval"), NumberStyles.Any, CultureInfo.InvariantCulture, out pollingValue))
            {
                pollingValue = 10;
            }
            PollIbs(pollingValue);
        }

        protected IUpdateOrderStatusJob StatusJobService { get; set; }

        private void PollIbs(int pollingValue)
        {
            Log.Info("Queue OrderStatusJob "+DateTime.Now.ToString("HH:MM:ss"));
            Observable.Timer(TimeSpan.FromSeconds(pollingValue))
                .Subscribe(_ =>
                {
                    try
                    {                        
                        StatusJobService.CheckStatus();
                    }
                    finally
                    {
                        PollIbs(pollingValue);
                    }
                });

        }



        protected void Session_Start(object sender, EventArgs e)
        {
             
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Request.Path.Contains(@"/api/"))
            {
                var watch = new Stopwatch();
                watch.Start();
                HttpContext.Current.Items.Add("RequestLoggingWatch", watch);
            }
        }

       
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            if (Request.Path.Contains(@"/api/"))
            {
                if (HttpContext.Current.Items["RequestLoggingWatch"] is Stopwatch)
                {
                                    
                    var watch = (Stopwatch) HttpContext.Current.Items["RequestLoggingWatch"];
                    watch.Stop();

                    var config = UnityServiceLocator.Instance.Resolve<IConfigurationManager>();
                    Log.Info(string.Format("[{2}] Request info : {0} completed in {1}ms ", Request.Path, watch.ElapsedMilliseconds, config.GetSetting("TaxiHail.ApplicationKey")));                    
                }
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}