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
                Database.SetInitializer<BookingDbContext>(null);
                Database.SetInitializer<ConfigurationDbContext>(null);

                containerFunq.Adapter = new UnityContainerAdapter(UnityServiceLocator.Instance, new Logger());
                var container = UnityServiceLocator.Instance;

                container.RegisterInstance<ILogger>(new Logger());

                container.RegisterType<BookingDbContext>(new TransientLifetimeManager(), new InjectionConstructor("MKWeb"));
                container.RegisterType<ConfigurationDbContext>(new TransientLifetimeManager(), new InjectionConstructor("MKWeb"));                
                container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());

                container.RegisterInstance<IAddressDao>(new AddressDao(() => container.Resolve<BookingDbContext>()));
                container.RegisterInstance<IAccountDao>(new AccountDao(() => container.Resolve<BookingDbContext>()));
                container.RegisterInstance<IConfigurationManager>(new Common.Configuration.Impl.ConfigurationManager(() => container.Resolve<ConfigurationDbContext>()));
                container.RegisterInstance<IAccountWebServiceClient>(new AccountWebServiceClient(container.Resolve<IConfigurationManager>(), new Logger()));
                container.RegisterInstance<IStaticDataWebServiceClient>(new StaticDataWebServiceClient(container.Resolve<IConfigurationManager>(), new Logger()));
                container.RegisterInstance<IBookingWebServiceClient>(new BookingWebServiceClient(container.Resolve<IConfigurationManager>(), new Logger()));

                container.RegisterInstance<IMessageSender>(new MessageSender(new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory),
                    ConfigurationManager.ConnectionStrings["DbContext.SqlBus"].ConnectionString, "SqlBus.Commands"));

                container.RegisterInstance<ICommandBus>(new CommandBus(container.Resolve<IMessageSender>(), container.Resolve<ITextSerializer>()));

                container.RegisterInstance<IPasswordService>(new PasswordService());
                container.RegisterInstance<ITemplateService>(new TemplateService());
                container.RegisterInstance<IEmailSender>(new EmailSender());
                

                Plugins.Add(new AuthFeature(() => new AuthUserSession(), new IAuthProvider[] { new CustomCredentialsAuthProvider(container.Resolve<IAccountDao>(), container.Resolve<IPasswordService>()) }));
                Plugins.Add(new ValidationFeature());
                containerFunq.RegisterValidators(typeof(SaveFavoriteAddressValidator).Assembly);

                container.RegisterInstance<ICacheClient>(new MemoryCacheClient{ FlushOnDispose = false });
                
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