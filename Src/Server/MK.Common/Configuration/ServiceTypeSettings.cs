using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Enumeration;

namespace MK.Common.Configuration
{
    public class ServiceTypeSettings
    {
        [Key]
        public ServiceType ServiceType { get; set; }

        public string IBSWebServicesUrl { get; set; }

        public int ProviderId { get; set; }

        public double WaitTimeRatePerMinute { get; set; }

        public double AirportMeetAndGreetRate { get; set; }

    }
}