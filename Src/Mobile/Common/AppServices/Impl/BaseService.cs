using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class BaseService: IUseServiceClient
    {
        
		protected static async Task<TResult> RunWithRetryAsync<TResult>(
			Func<Task<TResult>> action,
			TimeSpan retryInterval,
			Func<Exception, bool> stopCondition,
			int retryCount = 3) where TResult : class
		{
			var exceptions = new List<Exception>();

			var stop = false;

			for (var retry = 0; retry < retryCount && !stop; retry++)
			{
				try
				{
					var result = await Task.Run(action);
					return result;
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);

					stop = stopCondition(ex);
				}

				await Task.Delay(retryInterval);
			}

			throw new AggregateException(exceptions);
		}


		protected async Task<TResult> UseServiceClientAsync<TService, TResult>(Func<TService, Task<TResult>> action, Action<Exception> errorHandler = null, [CallerMemberName] string method = "") where TResult : class  where TService : class
        {
            var service = TinyIoCContainer.Current.Resolve<TService>();

            try
            {
                return await action(service)
                        .ConfigureAwait(false);
            }
            catch (Exception ex)
            {                    
                Logger.LogError(ex);
				bool handled;
				if (errorHandler == null)
				{
					handled = TinyIoCContainer.Current.Resolve<IErrorHandler> ().HandleError (ex);
				}
				else
				{
					errorHandler (ex);
					handled = true;
				} 
				if (!handled)
                {
					throw;
				}
                // this patch try to return empty typed list to avoid exceptions in program where result.FirstOrDefault calls happen
                // bad practice to rely on reflection should be replaced in future
                var result = CreateEmptyTypedArray<TResult>(null) as TResult;

                return result;
            }
        }

		protected async Task UseServiceClientAsync<TService>(Func<TService, Task> action, Action<Exception> errorHandler = null, [CallerMemberName] string method = "") where TService : class
		{
			var service = TinyIoCContainer.Current.Resolve<TService>();

			try
			{
				await action(service).ConfigureAwait(false);
			}
			catch (Exception ex)
			{                    
				Logger.LogError(ex);
				bool handled;
				if (errorHandler == null)
				{
					handled = TinyIoCContainer.Current.Resolve<IErrorHandler> ().HandleError (ex);
				}
				else
				{
					errorHandler (ex);
					handled = true;
				} 
				if (!handled)
                {
					throw;
				}
			}
		}

        private ILogger _logger;
        protected ILogger Logger
        {
            get
            {
				return _logger ?? (_logger = Mvx.Resolve<ILogger>());
            }
        }

		protected ICacheService UserCache
        {
            get
            {
                return TinyIoCContainer.Current.Resolve<ICacheService>("UserAppCache");
            }
        }

	    private static object CreateEmptyTypedArray<T>(Type type, int counter = 0)
        {
            if (counter > 5)
            {
                return null;
            }

            try
            {
                var genericType = type ?? typeof(T);

                if (!genericType.IsInterface || !genericType.IsGenericType || genericType.IsGenericTypeDefinition)
                {
                    return null;
                }
                    
                var typeInterfaces = genericType.GetInterfaces();

                if (typeInterfaces.None(t => t.Name == "IEnumerable"))
                {
                    return null;
                }

                if (!genericType.GenericTypeArguments[0].IsInterface)
                {
                    return Array.CreateInstance(genericType.GenericTypeArguments[0], 0);
                }

                var result = CreateEmptyTypedArray<T>(genericType.GenericTypeArguments[0], ++counter);
                return Array.CreateInstance(result.GetType(), 0);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}