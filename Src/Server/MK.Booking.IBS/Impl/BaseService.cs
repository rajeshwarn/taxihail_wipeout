﻿#region

using System;
using System.Web.Services.Protocols;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;

#endregion

namespace apcurium.MK.Booking.IBS.Impl
{
    public class BaseService<T> where T : SoapHttpClientProtocol, new()
    {
        public BaseService(IServerSettings configManager, ILogger logger)
        {
            Logger = logger;
            ConfigManager = configManager;
        }


        protected string UserNameApp
        {
            get { return ConfigManager.ServerData.IBS.WebServicesUserName; }
        }

        protected string PasswordApp
        {
            get { return ConfigManager.ServerData.IBS.WebServicesPassword; }
        }

        protected virtual string GetUrl()
        {
            return ConfigManager.ServerData.IBS.WebServicesUrl;
        }

        protected IServerSettings ConfigManager { get; set; }
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