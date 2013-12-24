#region

using System.Configuration;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using Microsoft.Practices.Unity;
using ServiceStack.CacheAccess;
using ConfigurationManager = apcurium.MK.Common.Configuration.Impl.ConfigurationManager;

#endregion

namespace apcurium.MK.Common
{
    public class Module
    {
        public const string MkConnectionString = "Mk.ConnectionString";

        public void Init(IUnityContainer container)
        {
            container.RegisterType<ConfigurationDbContext>(new TransientLifetimeManager(),
                new InjectionConstructor(
                    container.Resolve<ConnectionStringSettings>(MkConnectionString).ConnectionString));

            container.RegisterInstance<ILogger>(new Logger());
            container.RegisterInstance<IConfigurationManager>(
                new ConfigurationManager(() => container.Resolve<ConfigurationDbContext>()));

            container.RegisterType<CachingDbContext>(new TransientLifetimeManager(),
                new InjectionConstructor(
                    container.Resolve<ConnectionStringSettings>(MkConnectionString).ConnectionString));
            container.RegisterInstance<ICacheClient>(new EfCacheClient(() => container.Resolve<CachingDbContext>()));
        }
    }
}