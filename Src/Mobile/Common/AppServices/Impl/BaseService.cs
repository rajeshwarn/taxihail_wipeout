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
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class BaseService
    {

        protected void UseServiceClient<T>(Action<T> action) where T : class
        {
            try
            {
                var service = TinyIoCContainer.Current.Resolve<T>();
                action(service);                
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