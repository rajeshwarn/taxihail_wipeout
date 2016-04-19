﻿#region

using System.Reflection;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using System;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ApplicationInfoService : BaseApiService
    {
        private readonly IServerSettings _serverSettings;

        public ApplicationInfoService(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
        }

        public ApplicationInfo Get()
        {
            if (_serverSettings.ServerData.DisableNewerVersionPopup)
            {
                throw new Exception("Newer version popup is disabled.");
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