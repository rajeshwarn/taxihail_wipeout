using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using ServiceStack.CacheAccess;
using ServiceStack.DesignPatterns.Serialization;
using apcurium.MK.Common.Caching;
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

            Database.SetInitializer<CachingDbContext>(null);
            container.RegisterType<CachingDbContext>(new TransientLifetimeManager(), new InjectionConstructor("Caching"));
            container.RegisterInstance<ICacheClient>(new EFCacheClient(() => container.Resolve<CachingDbContext>()));
        }
    }
}
