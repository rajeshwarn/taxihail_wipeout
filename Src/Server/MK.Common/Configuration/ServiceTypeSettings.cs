using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Enumeration;

namespace MK.Common.Configuration
{
    public class ServiceTypeSettings
    {
        [Key]
        public Guid Id { get; set; }

        public ServiceType ServiceType { get; set; }

        public string IBSWebServicesUrl { get; set; }

        public int FutureBookingThresholdInMinutes { get; set; }
    }
}