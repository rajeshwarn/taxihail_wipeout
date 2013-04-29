using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Optimization;
using Microsoft.Practices.Unity;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.Text;
using ServiceStack.Text.Common;
using ServiceStack.WebHost.Endpoints;
using Funq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Jobs;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Validation;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Security;
using apcurium.MK.Common.IoC;
using log4net.Config;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;
using System.Threading;

namespace apcurium.MK.Web
{
    public class Global : System.Web.HttpApplication
    {
        private const string CacheKey = "OrderStatusJob";

        public class MKWebAppHost : AppHostBase
        {
            

            public MKWebAppHost() : base("Mobile Knowledge Web Services", typeof(CurrentAccountService).Assembly)
            {

                ServiceStack.Text.JsConfig.Reset();
                ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;
                ServiceStack.Text.JsConfig.DateHandler = JsonDateHandler.ISO8601;
                ServiceStack.Text.JsConfig<DateTime?>.RawDeserializeFn = NullableDateTimeRawDesirializtion;

            }

            private DateTime? NullableDateTimeRawDesirializtion(string s)
            {
                try
                {
                    if (s.IndexOf(".") > 0)
                    {
                        s = s.Substring(0, s.IndexOf("."));
                    }
                    return DateTimeSerializer.ParseShortestXsdDateTime(s);
                }
                catch (Exception)
                {
                    return null;
                }
                throw new NotImplementedException();

            }

            public override void Configure(Container containerFunq)
            {
                Trace.WriteLine("Configure AppHost");
                new Module().Init(UnityServiceLocator.Instance);

                var container = UnityServiceLocator.Instance;
                containerFunq.Adapter = new UnityContainerAdapter(container, new Logger());

                Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                    new IAuthProvider[]
                    {
                        new CustomCredentialsAuthProvider(container.Resolve<IAccountDao>(), container.Resolve<IPasswordService>()),
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

                


                Trace.WriteLine("Configure AppHost finished");
            }
            
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            XmlConfigurator.Configure();
            new MKWebAppHost().Init();
            if (HttpRuntime.Cache[CacheKey] == null)
            {
                Trace.WriteLine("Add OrderStatusJob in Cache");
                HttpRuntime.Cache.Insert(CacheKey, new object(), null, DateTime.Now.AddSeconds(10), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, CacheItemRemoved);
            }
        }

        private void CacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            try
            {
                Trace.WriteLine("Check Order Status");
                var statusJobService = UnityServiceLocator.Instance.Resolve<IUpdateOrderStatusJob>();
                statusJobService.CheckStatus();
            }
            finally
            {
                HttpRuntime.Cache.Insert(CacheKey, new object(), null, DateTime.Now.AddSeconds(10), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, CacheItemRemoved);
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {
             
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            
            Trace.WriteLine("Request Begin");
        }

       
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            Trace.WriteLine("End Requestn");
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