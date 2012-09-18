using System;
using System.Diagnostics;
using System.IO;
using System.Web;
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
using log4net.Config;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web
{
    public class Global : System.Web.HttpApplication
    {
        public class MKWebAppHost : AppHostBase
        {
            public MKWebAppHost() : base("Mobile Knowledge Web Services", typeof(CurrentAccountService).Assembly) { }

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

                ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;

                Trace.WriteLine("Configure AppHost finished");
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            XmlConfigurator.Configure();
            new MKWebAppHost().Init();
            
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