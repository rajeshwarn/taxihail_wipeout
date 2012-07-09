using TaxiMobile.Lib.Infrastructure;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services;
using TaxiMobile.Lib.Services.Impl;

namespace TaxiMobile.Lib.Tests
{
    public class BaseTest
    {
        protected void Setup()
        {
            ServiceLocator.Current.Register<IAppSettings, AppSettings>();
            ServiceLocator.Current.Register<ILogger, SimpleLogger>();
            ServiceLocator.Current.Register<IAppContext, TestAppContext>();
            ServiceLocator.Current.Register<IStaticDataService, StaticDataServiceImpl>();
        }
    }
}