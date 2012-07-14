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
using TinyIoC;
using apcurium.MK.Booking.Mobile.Practices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client
{
    [Application(Name = "com.apcurium.MK.TaxiHail")]
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

            Console.WriteLine("App created");

            new Bootstrapper(new IModule[] { new AppModule(this) }).Run();

        }

        
        private void AndroidEnvironmentOnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs raiseThrowableEventArgs)
        {
            try
            {
                TinyIoCContainer.Current.Resolve<ILogger>().LogError( raiseThrowableEventArgs.Exception );
            }
            catch (Exception)
            {
                

                throw;
            }
        }
    }
}