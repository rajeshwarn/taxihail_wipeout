using System;
using Android.App;
using Android.Runtime;
using TaxiMobile.Lib.Infrastructure;
using TaxiMobile.Lib.Practices;

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

            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentOnUnhandledExceptionRaiser;
            
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