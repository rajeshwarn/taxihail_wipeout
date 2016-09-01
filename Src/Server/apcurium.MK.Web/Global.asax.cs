#region

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.IoC;
using apcurium.MK.Web;
using log4net;
using log4net.Config;
using MK.Common.Configuration;
using UnityContainerExtensions = Microsoft.Practices.Unity.UnityContainerExtensions;

#endregion

namespace apcurium.MK.Web
{
    public class Global : System.Web.HttpApplication
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Global));

        private int _defaultPollingValue;

        private const int WaitingForPaymentPollingValue = 2;

        protected void Application_Start(object sender, EventArgs e)
        {
            XmlConfigurator.Configure();
            new MkWebAppHost().Init();

            var config = UnityContainerExtensions.Resolve<IServerSettings>(UnityServiceLocator.Instance);
            BundleConfig.RegisterBundles(BundleTable.Bundles, config.ServerData.TaxiHail.ApplicationKey);

            var serverSettings = UnityContainerExtensions.Resolve<IServerSettings>(UnityServiceLocator.Instance);

            _defaultPollingValue = serverSettings.ServerData.OrderStatus.ServerPollingInterval;
            PollIbs(_defaultPollingValue);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        }

        private void PollIbs(int pollingValue)
        {
            Log.Info("Queue OrderStatusJob " + DateTime.Now.ToString("HH:MM:ss"));

            var hasOrdersWaitingForPayment = false;

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
                    catch (Exception ex)
                    {
                        Log.Error(ex);
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

        private bool _firstRequest = true;
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (_firstRequest)
            {
                var notificationService = UnityContainerExtensions.Resolve<INotificationService>(UnityServiceLocator.Instance);
                var appSettings = UnityContainerExtensions.Resolve<IServerSettings>(UnityServiceLocator.Instance);

                notificationService.SetBaseUrl(appSettings.ServerData.BaseUrl.HasValue()
                            ? new Uri(appSettings.ServerData.BaseUrl)
                            : new Uri(Request.Url, VirtualPathUtility.ToAbsolute("~")));

                appSettings.ServerData.Target = ResolveDeploymentTarget(Request.Url.Host);

                _firstRequest = false;
            }
            if (Request.Path.Contains(@"/api/"))
            {
                var watch = new Stopwatch();
                watch.Start();
                HttpContext.Current.Items.Add("RequestLoggingWatch", watch);
            }
        }

        private DeploymentTargets ResolveDeploymentTarget(string host)
        {
            var caseInsensitiveHost = host.ToLower();

            if (caseInsensitiveHost.Contains("localhost"))
            {
                return DeploymentTargets.Local;
            }
            if (caseInsensitiveHost.Contains("test.taxihail.biz"))
            {
                return DeploymentTargets.Dev;
            }
            if (caseInsensitiveHost.Contains("staging"))
            {
                return DeploymentTargets.Staging;
            }

            return DeploymentTargets.Production;
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            if (Request.Path.Contains(@"/api/"))
            {
                if (HttpContext.Current.Items["RequestLoggingWatch"] is Stopwatch)
                {
                    var watch = (Stopwatch)HttpContext.Current.Items["RequestLoggingWatch"];
                    watch.Stop();

                    var config = UnityContainerExtensions.Resolve<IServerSettings>(UnityServiceLocator.Instance);
                    Log.Info(string.Format("[{2}] Request info : {0} completed in {1}ms ", Request.Path,
                        watch.ElapsedMilliseconds, config.ServerData.TaxiHail.ApplicationKey));
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