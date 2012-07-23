using System;
using Microsoft.Practices.Unity;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.WebHost.Endpoints;
using Funq;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Validation;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Database;
using Infrastructure.Messaging;
using Infrastructure.Sql.Messaging;
using Infrastructure.Sql.Messaging.Implementation;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using Infrastructure.Serialization;
using System.Data.Entity;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Security;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using apcurium.MK.Common.IoC;
using ConfigurationManager = System.Configuration.ConfigurationManager;
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
                new Module().Init(UnityServiceLocator.Instance);

                var container = UnityServiceLocator.Instance;
                containerFunq.Adapter = new UnityContainerAdapter(container, new Logger());

                Plugins.Add(new AuthFeature(() => new AuthUserSession(), new IAuthProvider[] { new CustomCredentialsAuthProvider(container.Resolve<IAccountDao>(), container.Resolve<IPasswordService>()) }));
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

                
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            new MKWebAppHost().Init();
        }
        
        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
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