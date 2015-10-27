using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Provider;
using MK.Common.Configuration;

namespace apcurium.MK.Booking.Services.Impl
{
    public class ServiceTypeSettingsProvider : IServiceTypeSettingsProvider
    {
        private readonly IServerSettings _serverSettings;
        private readonly Func<ConfigurationDbContext> _contextFactory;

        public ServiceTypeSettingsProvider(Func<ConfigurationDbContext> contextFactory, IServerSettings serverSettings)
        {
            _contextFactory = contextFactory;
            _serverSettings = serverSettings;
        }

        public IBSSettingContainer GetIBSSettings(ServiceType serviceType)
        {
            var ibsSettingsContainer = _serverSettings.ServerData.IBS;

            ibsSettingsContainer.WebServicesUrl = GetSettings(serviceType).IBSWebServicesUrl;

            return ibsSettingsContainer;
        }

        public ServiceTypeSettings GetSettings(ServiceType serviceType)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.ServiceTypeSettings.Find(serviceType);
            }
        }

        public IEnumerable<ServiceTypeSettings> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.ServiceTypeSettings.ToList();
            }
        }
    }
}