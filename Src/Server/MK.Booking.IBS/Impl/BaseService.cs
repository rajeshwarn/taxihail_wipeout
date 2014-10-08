#region

using System;
using System.Web.Services.Protocols;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;

#endregion

namespace apcurium.MK.Booking.IBS.Impl
{
    public class BaseService<T> where T : SoapHttpClientProtocol, new()
    {
        public BaseService(IServerSettings serverSettings, ILogger logger)
        {
            Logger = logger;
            ServerSettings = serverSettings;
        }
        
        protected string UserNameApp
        {
            get { return ServerSettings.ServerData.IBS.WebServicesUserName; }
        }

        protected string PasswordApp
        {
            get { return ServerSettings.ServerData.IBS.WebServicesPassword; }
        }

        protected virtual string GetUrl()
        {
            return ServerSettings.ServerData.IBS.WebServicesUrl;
        }

        protected IServerSettings ServerSettings { get; set; }
        protected ILogger Logger { get; set; }

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
    }
}