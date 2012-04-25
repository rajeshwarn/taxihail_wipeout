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

namespace TaxiMobile
{
    [Application(Name = "com.apcurium.TaxiMobile")]
    public class TaxiMobileApplication : Application
    {
        protected TaxiMobileApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {

        }


        public TaxiMobileApplication()
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();

            Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentOnUnhandledExceptionRaiser;
            
            new Bootstrapper(new IModule[] { new AppModule(this) }).Run();

        }

        private void AndroidEnvironmentOnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs raiseThrowableEventArgs)
        {
            try
            {
                ServiceLocator.Current.GetInstance<ILogger>().LogError( raiseThrowableEventArgs.Exception );
            }
            catch (Exception)
            {
                

                throw;
            }
        }
    }
}