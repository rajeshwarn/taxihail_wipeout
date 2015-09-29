#region

using System.Reflection;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using ServiceStack.ServiceInterface;
using System;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ApplicationInfoService : Service
    {
        private readonly IServerSettings _serverSettings;

        public ApplicationInfoService(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
        }

        public object Get(ApplicationInfoRequest request)
        {
            if (_serverSettings.ServerData.DisableNewerVersionPopup)
            {
                throw new Exception();
            }

            var info = new ApplicationInfo
            {
                Version = Assembly.GetAssembly(typeof(ApplicationInfoService)).GetName().Version.ToString(),
                SiteName = _serverSettings.ServerData.TaxiHail.SiteName,
				MinimumRequiredAppVersion = _serverSettings.ServerData.MinimumRequiredAppVersion
            };
            return info;
        }
    }
}