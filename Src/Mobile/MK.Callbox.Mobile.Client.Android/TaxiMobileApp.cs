using System;
using Android.App;
using Android.Runtime;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;

namespace apcurium.MK.Callbox.Mobile.Client
{
    [Application]
    public class TaxiMobileApplication : Application
    {
        protected TaxiMobileApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public TaxiMobileApplication()
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentOnUnhandledExceptionRaiser;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception)
            {
                Mvx.Resolve<ILogger>().LogError((Exception)e.ExceptionObject);
            }
        }

        private void AndroidEnvironmentOnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs raiseThrowableEventArgs)
        {
            Mvx.Resolve<ILogger>().LogError( raiseThrowableEventArgs.Exception );
        }
    }
}