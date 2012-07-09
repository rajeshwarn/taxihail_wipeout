using TaxiMobile.Diagnostic;
using TaxiMobile.Lib.Infrastructure;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Localization;

namespace TaxiMobile
{
    public class AppModule : IModule
    {

        public AppModule(TaxiMobileApplication app)
        {

            App = app;
            app.PackageManager.GetPackageInfo(app.PackageName, 0);
        }

        public TaxiMobileApplication App { get; set; }
        public void Initialize()
        {
            ServiceLocator.Current.Register<IAppResource, ResourceManager>();
            ServiceLocator.Current.RegisterSingleInstance2<IAppSettings>(new AppSettings(App));
            ServiceLocator.Current.Register<ILogger,LoggerImpl>();
            ServiceLocator.Current.RegisterSingleInstance2<IAppContext>(new AppContext(App));
        
        }
    }
}