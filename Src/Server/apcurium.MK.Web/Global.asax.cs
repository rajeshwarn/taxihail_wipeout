using System;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using System.Web;
using System.Web.Optimization;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.Text;
using ServiceStack.Text.Common;
using ServiceStack.WebHost.Endpoints;
using Funq;
using apcurium.MK.Booking.Api.Jobs;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Validation;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Security;
using apcurium.MK.Common.IoC;
using log4net.Config;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web
{
    public class Global : HttpApplication
    {

        public class MKWebAppHost : AppHostBase
        {
            public MKWebAppHost() : base("Mobile Knowledge Web Services", typeof(CurrentAccountService).Assembly)
            {

                JsConfig.Reset();
                JsConfig.EmitCamelCaseNames = true;
                JsConfig.DateHandler = JsonDateHandler.ISO8601;
                JsConfig<DateTime?>.RawDeserializeFn = NullableDateTimeRawDesirializtion;


            }
            


            private DateTime? NullableDateTimeRawDesirializtion(string s)
            {
                try
                {
                    if (s.IndexOf('.') > 0)
                    {
                        s = s.Substring(0, s.IndexOf('.'));
                    }
                    return DateTimeSerializer.ParseShortestXsdDateTime(s);
                }
                catch (Exception)
                {
                    return null;
                }
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
                        new CustomCredentialsAuthProvider(container.Resolve<ICommandBus>(), container.Resolve<IAccountDao>(), container.Resolve<IPasswordService>()),
                        new CustomFacebookAuthProvider(container.Resolve<IAccountDao>()), 
                        new CustomTwitterAuthProvider(container.Resolve<IAccountDao>()), 
                    }));

                Plugins.Add(new ValidationFeature());
                containerFunq.RegisterValidators(typeof(SaveFavoriteAddressValidator).Assembly);
                
                RequestFilters.Add((httpReq, httpResp, requestDto) =>
                {
                    var authSession = httpReq.GetSession();
                    if (authSession != null && authSession.UserAuthId != null)
                    {
                        var account = container.Resolve<IAccountDao>().FindById(new Guid(authSession.UserAuthId));
                        if (account.DisabledByAdmin)
                        {
                            httpReq.RemoveSession();
                        }
                    }
                });

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
            XmlConfigurator.Configure();
            new MKWebAppHost().Init();

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
            Trace.WriteLine("Queue OrderStatusJob "+DateTime.Now.ToString("HH:MM:ss"));
            Observable.Timer(TimeSpan.FromSeconds(pollingValue))
                .Subscribe(_ =>
                {
                    try
                    {
                        Trace.WriteLine("Check Order Status " + DateTime.Now.ToString("HH:MM:ss"));
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
        }

       
        protected void Application_EndRequest(object sender, EventArgs e)
        {         
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