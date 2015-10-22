using System.Collections.Generic;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Services.Impl
{
    public class ServiceTypeSettingsProvider : IServiceTypeSettingsProvider
    {
        private readonly IServerSettings _serverSettings;
        private Dictionary<ServiceType, ServiceTypeSettings> _serviceTypeSettings;

        public ServiceTypeSettingsProvider(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
            _serviceTypeSettings = new Dictionary<ServiceType, ServiceTypeSettings>
            {
                {
                    ServiceType.Taxi, new ServiceTypeSettings
                    {
                        IBSWebServicesUrl = "ibs1.com",
                        FutureBookingThresholdInMinutes = 0
                    }
                },
                {
                    ServiceType.Luxury, new ServiceTypeSettings
                    {
                        IBSWebServicesUrl = "ibs2.com",
                        FutureBookingThresholdInMinutes = 15
                    }
                }
            };
        }

        public IBSSettingContainer GetIBSSettings(ServiceType serviceType)
        {
            var ibsSettingsContainer = _serverSettings.ServerData.IBS;

            ibsSettingsContainer.WebServicesUrl = _serviceTypeSettings[serviceType].IBSWebServicesUrl;

            return ibsSettingsContainer;
        }

        private class ServiceTypeSettings
        {
            public string IBSWebServicesUrl { get; set; }
            public int FutureBookingThresholdInMinutes { get; set; }
        }
    }
}