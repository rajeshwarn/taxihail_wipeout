using System;
using Android.App;
using Android.Runtime;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;

namespace apcurium.MK.Callbox.Mobile.Client
{
    [Application]
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

            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentOnUnhandledExceptionRaiser;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Console.WriteLine("App created");

        }

		public override void OnTerminate ()
		{
			base.OnTerminate ();
		}

        
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception)
                {
					Mvx.Resolve<ILogger>().LogError((Exception)e.ExceptionObject);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        
        private void AndroidEnvironmentOnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs raiseThrowableEventArgs)
        {
            try
            {
				Mvx.Resolve<ILogger>().LogError( raiseThrowableEventArgs.Exception );
            }
            catch (Exception)
            {
                throw;
            }
        }
    }


}