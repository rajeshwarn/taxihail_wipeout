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
        protected IBSSettingContainer _ibsSettings;

        public BaseService(IBSSettingContainer ibsSettings, ILogger logger)
        {
            Logger = logger;
            _ibsSettings = ibsSettings;
        }
        
        protected string UserNameApp
        {
            get { return _ibsSettings.WebServicesUserName; }
        }

        protected string PasswordApp
        {
            get { return _ibsSettings.WebServicesPassword; }
        }

        protected virtual string GetUrl()
        {
            return _ibsSettings.WebServicesUrl;
        }

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