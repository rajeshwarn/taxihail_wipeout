using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Infrastructure.Messaging;
using Infrastructure.Serialization;
using Infrastructure.Sql.Messaging;
using Infrastructure.Sql.Messaging.Implementation;
using Microsoft.Practices.Unity;
using apcurium.MK.Booking.Database;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace apcurium.MK.Web
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            Database.SetInitializer<BookingDbContext>(null);
            Database.SetInitializer<ConfigurationDbContext>(null);
            container.RegisterType<BookingDbContext>(new TransientLifetimeManager(), new InjectionConstructor("MKWeb"));
            container.RegisterType<ConfigurationDbContext>(new TransientLifetimeManager(), new InjectionConstructor("MKWeb"));

            // Infrastructure
            container.RegisterInstance<ITextSerializer>(new JsonTextSerializer());

            // Common
            container.RegisterInstance<IConfigurationManager>(new Common.Configuration.Impl.ConfigurationManager(() => container.Resolve<ConfigurationDbContext>()));
            container.RegisterInstance<ILogger>(new Logger());


            new MK.Booking.Module().Init(container);
            new MK.Booking.IBS.Module().Init(container);
            new MK.Booking.Api.Module().Init(container);

            RegisterCommandBus(container);

        }

        private void RegisterCommandBus(IUnityContainer container)
        {
            container.RegisterInstance<IMessageSender>(new MessageSender(new ServiceConfigurationSettingConnectionFactory(Database.DefaultConnectionFactory),
                   ConfigurationManager.ConnectionStrings["DbContext.SqlBus"].ConnectionString, "SqlBus.Commands"));

            container.RegisterInstance<ICommandBus>(new CommandBus(container.Resolve<IMessageSender>(), container.Resolve<ITextSerializer>()));
        }

    }
}