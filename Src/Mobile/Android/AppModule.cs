using TinyIoC;
using apcurium.MK.Booking.Mobile.Practices;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Cache;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client
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
            TinyIoCContainer.Current.Register<IAppSettings>( new AppSettings(App));
            TinyIoCContainer.Current.Register<IAppContext>(new AppContext(App));
            TinyIoCContainer.Current.Register<IAppResource, ResourceManager>();
            TinyIoCContainer.Current.Register<ILogger, LoggerImpl>();

            TinyIoCContainer.Current.Register<ICacheService>(new CacheService(App));
        
        }
    }
}