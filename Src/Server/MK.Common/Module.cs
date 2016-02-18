#region

using System.Configuration;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using Microsoft.Practices.Unity;

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
            container.RegisterInstance<IClock>(new SystemClock());

            var settings = new ServerSettings(() => container.Resolve<ConfigurationDbContext>(),
                container.Resolve<ILogger>());

            container.RegisterInstance<IServerSettings>(settings);
            container.RegisterInstance<IAppSettings>(settings);
            
            container.RegisterType<CachingDbContext>(new TransientLifetimeManager(),
                new InjectionConstructor(
                    container.Resolve<ConnectionStringSettings>(MkConnectionString).ConnectionString));
            //TODO MKTAXI-3370: ICacheClient
            //container.RegisterInstance<ICacheClient>(new EfCacheClient(() => container.Resolve<CachingDbContext>()));
        }
    }
}