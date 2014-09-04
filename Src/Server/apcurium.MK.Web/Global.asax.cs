#region

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using System.Web;
using System.Web.Optimization;
using apcurium.MK.Booking.Api.Jobs;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.IoC;
using apcurium.MK.Web.App_Start;
using log4net;
using log4net.Config;
using UnityContainerExtensions = Microsoft.Practices.Unity.UnityContainerExtensions;

#endregion

namespace apcurium.MK.Web
{
    public class Global : HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Global));

        private int _defaultPollingValue;

        private const int WaitingForPaymentPollingValue = 2;

        protected void Application_Start(object sender, EventArgs e)
        {
            XmlConfigurator.Configure();
            new MkWebAppHost().Init();

            var config = UnityContainerExtensions.Resolve<IConfigurationManager>(UnityServiceLocator.Instance);
            BundleConfig.RegisterBundles(BundleTable.Bundles, config.GetSetting("TaxiHail.ApplicationKey"));

            _defaultPollingValue = config.GetSetting("OrderStatus.ServerPollingInterval", 10);
            PollIbs(_defaultPollingValue);
        }

        private void PollIbs(int pollingValue)
        {
            Log.Info("Queue OrderStatusJob " + DateTime.Now.ToString("HH:MM:ss"));

            bool hasOrdersWaitingForPayment = false;

            Observable.Timer(TimeSpan.FromSeconds(pollingValue))
                .Subscribe(_ =>
                {
                    try
                    {
                        var serverProcessId = GetServerProcessId();
                        Trace.WriteLine("serverProcessId : " + serverProcessId);
                        var statusJobService = UnityContainerExtensions.Resolve<IUpdateOrderStatusJob>(UnityServiceLocator.Instance);
                        hasOrdersWaitingForPayment = statusJobService.CheckStatus(serverProcessId, pollingValue);
                    }
                    finally
                    {
                        PollIbs(hasOrdersWaitingForPayment ? WaitingForPaymentPollingValue : _defaultPollingValue);
                    }
                });
        }

        private string GetServerProcessId()
        {
            return string.Format("{0}_{1}", Environment.MachineName, Process.GetCurrentProcess().Id);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (!UnityContainerExtensions.IsRegistered<string>(UnityServiceLocator.Instance, "BaseUrl"))
            {
                UnityContainerExtensions.RegisterInstance<string>(UnityServiceLocator.Instance, "BaseUrl", new Uri(Request.Url, VirtualPathUtility.ToAbsolute("~")).ToString());
            }
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
                    var watch = (Stopwatch)HttpContext.Current.Items["RequestLoggingWatch"];
                    watch.Stop();

                    var config = UnityContainerExtensions.Resolve<IConfigurationManager>(UnityServiceLocator.Instance);
                    Log.Info(string.Format("[{2}] Request info : {0} completed in {1}ms ", Request.Path,
                        watch.ElapsedMilliseconds, config.GetSetting("TaxiHail.ApplicationKey")));
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