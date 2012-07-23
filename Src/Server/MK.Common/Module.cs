using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Common
{
    public class Module
    {
        public void Init(IUnityContainer container)
        {
            Database.SetInitializer<ConfigurationDbContext>(null);
            container.RegisterType<ConfigurationDbContext>(new TransientLifetimeManager(), new InjectionConstructor("Configuration"));

            container.RegisterInstance<ILogger>(new Logger());
            container.RegisterInstance<IConfigurationManager>(new Common.Configuration.Impl.ConfigurationManager(() => container.Resolve<ConfigurationDbContext>()));
        }
    }
}
