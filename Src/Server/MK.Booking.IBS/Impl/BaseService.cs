using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;
using apcurium.MK.Common.Diagnostic;

using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.IBS.Impl
{

    public class BaseService<T> where T : SoapHttpClientProtocol, new()
    {
        protected string _userNameApp;
        protected string _passwordApp;
        
        private ILogger _logger;
        public BaseService(IConfigurationManager configManager, ILogger logger)
        {
            Logger = logger;
            ConfigManager = configManager;
            _userNameApp = ConfigManager.GetSetting("IBS.WebServicesUserName");
            _passwordApp = ConfigManager.GetSetting("IBS.WebServicesPassword");
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
