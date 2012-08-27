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
using apcurium.MK.Booking.Mobile.Client.Services;

namespace apcurium.MK.Booking.Mobile.Client
{
    [Application(Name = "com.apcurium.MK.TaxiHail")]
    public class TaxiMobileApplication : Application
    {
		private Intent _errorHandlingServiceIntent;

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

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Console.WriteLine("App created");

			_errorHandlingServiceIntent = new Intent( this, typeof(ErrorHandlingService) );
			StartService( _errorHandlingServiceIntent );

            new Bootstrapper(new IModule[] { new AppModule(this) }).Run();

        }

		public override void OnTerminate ()
		{
			base.OnTerminate ();

			StopService( _errorHandlingServiceIntent );
		}

        
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception)
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogError((Exception)e.ExceptionObject);
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
                TinyIoCContainer.Current.Resolve<ILogger>().LogError( raiseThrowableEventArgs.Exception );
            }
            catch (Exception)
            {
                

                throw;
            }
        }
    }


}