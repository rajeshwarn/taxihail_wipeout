using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Threading;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class BaseService: IUseServiceClient
    {

        protected string UseServiceClient<T>(Action<T> action, Action<Exception> errorHandler = null) where T : class
        {
            return this.UseServiceClient<T>(null, action, errorHandler);
        }

        protected string UseServiceClient<T>( string name, Action<T> action, Action<Exception> errorHandler = null ) where T : class
        {
			return IUseServiceClientExtensions.UseServiceClient(this, name, action, errorHandler);
        }

        protected void QueueCommand<T>(Action<T> action) where T : class
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    UseServiceClient(action);
                }
                catch (Exception ex)
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
                }
            });
            
        }



    }
}