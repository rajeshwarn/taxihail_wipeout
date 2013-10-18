using System;
using System.Web.Services.Protocols;
using apcurium.MK.Common.Diagnostic;

using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.IBS.Impl
{

    public class BaseService<T> where T : SoapHttpClientProtocol, new()
    {
        protected string UserNameApp;
        protected string PasswordApp;
        
        public BaseService(IConfigurationManager configManager, ILogger logger)
        {
            Logger = logger;
            ConfigManager = configManager;
            UserNameApp = ConfigManager.GetSetting("IBS.WebServicesUserName");
            PasswordApp = ConfigManager.GetSetting("IBS.WebServicesPassword");
        }

        protected IConfigurationManager ConfigManager { get; set; }
        protected ILogger Logger { get; set; }

        protected void UseService(Action<T> action)
        {
            var service = new T { Url = GetUrl() };

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
            return ConfigManager.GetSetting("IBS.WebServicesUrl");
        }

        
    }

}
