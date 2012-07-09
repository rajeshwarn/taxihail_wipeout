using System;
using System.Web.Services.Protocols;
using TaxiMobile.Lib.Infrastructure;
using TaxiMobile.Lib.Practices;

namespace TaxiMobile.Lib.Services.Impl
{
    public class BaseService<T> where T : SoapHttpClientProtocol, new()
    {
        protected string userNameApp = "taxi";
        protected string passwordApp = "test";

        protected void UseService(Action<T> action)
        {
            var service = new T {Url = GetUrl()};

            try
            {
                action(service);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception);
            }
            finally
            {
                service.Abort();
                service.Dispose();
            }
        }

        protected virtual string GetUrl()
        {
            return ServiceLocator.Current.GetInstance<IAppSettings>().ServiceUrl;
        }

        protected ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }
    }
}