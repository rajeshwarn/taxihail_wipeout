using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TaxiMobileApp;
using Microsoft.Practices.ServiceLocation;
using TaxiMobile.Localization;
using MobileTaxiApp.Infrastructure;
using TaxiMobile.Diagnostic;

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