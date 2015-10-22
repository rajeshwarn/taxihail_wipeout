using System.Collections.Generic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Services.Impl
{
    public class ServiceTypeSettingsProvider : IServiceTypeSettingsProvider
    {
        private Dictionary<ServiceType, ServiceTypeSettings> _serviceTypeSettings;

        public ServiceTypeSettingsProvider()
        {
            _serviceTypeSettings = new Dictionary<ServiceType, ServiceTypeSettings>();

            _serviceTypeSettings.Add(ServiceType.Taxi, new ServiceTypeSettings
            {
                IBSWebServicesUrl = "ibs1.com"
            });

            _serviceTypeSettings.Add(ServiceType.Luxury, new ServiceTypeSettings
            {
                IBSWebServicesUrl = "ibs2.com"
            });
        }

        public string GetIBSWebServicesUrl(ServiceType serviceType)
        {
            return _serviceTypeSettings[serviceType].IBSWebServicesUrl;
        }

        private class ServiceTypeSettings
        {
            public string IBSWebServicesUrl { get; set; }
        }
    }
}