using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using ServiceStack.WebHost.Endpoints;
using Funq;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Database;
using Infrastructure.Messaging;
using Infrastructure.Sql.Messaging;
using Infrastructure.Sql.Messaging.Implementation;
using apcurium.MK.Common.Entity;
using System.Configuration;
using Infrastructure.Serialization;
using System.Data.Entity;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Security;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;


namespace apcurium.MK.Web
{
    public class Global : System.Web.HttpApplication
    {

        public class MKWebAppHost : AppHostBase
        {

            public MKWebAppHost() : base("Mobile Knowledge Web Services", typeof(CurrentAccountService).Assembly) { }

            public override void Configure(Container container)
            {
                ////register user-defined REST-ful urls
                //Routes
                //  .Add<Hello>("/hello")
                //  .Add<Hello>("/hello/{Name}");

                
                
                container.Register<BookingDbContext>(c => new BookingDbContext("MKWeb")).ReusedWithin(ReuseScope.None);
                container.Register<IAccountDao>(c => new AccountDao(() => c.Resolve<BookingDbContext>())).ReusedWithin(ReuseScope.None);
                container.Register<ITextSerializer>(new JsonTextSerializer());

                container.Register<IMessageSender>(c => new MessageSender(new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory),
                    ConfigurationManager.ConnectionStrings["DbContext.SqlBus"].ConnectionString, "SqlBus.Commands")).ReusedWithin(ReuseScope.None);

                container.Register<ICommandBus>(c => new CommandBus(c.Resolve<IMessageSender>(), c.Resolve<ITextSerializer>())).ReusedWithin(ReuseScope.Container);


                Plugins.Add(new AuthFeature(() => new AuthUserSession(), new IAuthProvider[] { new CustomCredentialsAuthProvider(container.Resolve <IAccountDao>()) }));

                container.Register<ICacheClient>(new MemoryCacheClient() { FlushOnDispose = false });
                
                SetConfig(new EndpointHostConfig
                {
                    GlobalResponseHeaders =
                        {
                            { "Access-Control-Allow-Origin", "*" },
                            { "Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS" },
                        },
                });
                
                


                //container.Register<BookingDbContext>(c => new BookingDbContext("MKWeb")).ReusedWithin(ReuseScope.None);
                //container.Register<IAccountDao>(c => new AccountDao(() => c.Resolve<BookingDbContext>())).ReusedWithin(ReuseScope.None);
                //container.Register<ITextSerializer>(new JsonTextSerializer());

                //container.Register<IMessageSender>(c => new MessageSender(new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory),
                //    ConfigurationManager.ConnectionStrings["DbContext.SqlBus"].ConnectionString, "SqlBus.Commands")).ReusedWithin(ReuseScope.None);

                //container.Register<ICommandBus>(c => new CommandBus(c.Resolve<IMessageSender>(), c.Resolve<ITextSerializer>())).ReusedWithin(ReuseScope.Container);
                
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