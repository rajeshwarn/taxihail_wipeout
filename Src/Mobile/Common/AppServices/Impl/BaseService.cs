using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TinyIoC;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class BaseService
    {

        protected void UseServiceClient<T>(Action<T> action) where T : class
        {
            try
            {
                TinyIoCContainer.Current.Resolve<ILogger>().StartStopwatch("UseServiceClient : " + typeof(T));
                var service = TinyIoCContainer.Current.Resolve<T>();
                action(service);
                TinyIoCContainer.Current.Resolve<ILogger>().StopStopwatch("UseServiceClient : " + typeof(T));
            }
            catch (Exception ex)
            {
                TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);                
            }
        }


        protected void QueueCommand<T>(Action<T> action) where T : class
        {
            
            UseServiceClient(action);
        }



    }
}