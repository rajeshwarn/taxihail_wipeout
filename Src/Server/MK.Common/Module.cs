using System.Configuration;
using System.Data.Entity;
using Microsoft.Practices.Unity;
using ServiceStack.CacheAccess;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Common
{
    public class Module
    {
        public const string MKConnectionString = "Mk.ConnectionString";

        public void Init(IUnityContainer container)
        {
            Database.SetInitializer<ConfigurationDbContext>(null);
            container.RegisterType<ConfigurationDbContext>(new TransientLifetimeManager(), new InjectionConstructor(container.Resolve<ConnectionStringSettings>(apcurium.MK.Common.Module.MKConnectionString).ConnectionString));

            container.RegisterInstance<ILogger>(new Logger());
            container.RegisterInstance<IConfigurationManager>(new Common.Configuration.Impl.ConfigurationManager(() => container.Resolve<ConfigurationDbContext>()));

            Database.SetInitializer<CachingDbContext>(null);
            container.RegisterType<CachingDbContext>(new TransientLifetimeManager(), new InjectionConstructor(container.Resolve<ConnectionStringSettings>(apcurium.MK.Common.Module.MKConnectionString).ConnectionString));
            container.RegisterInstance<ICacheClient>(new EFCacheClient(() => container.Resolve<CachingDbContext>()));
        }
    }
}
