using System;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Events
{
    public class ManualRideLinqPaired : VersionedEvent
    {
        public Guid AccountId { get; set; }
        public string RideLinqId { get; set; }
        public DateTime StartTime { get; set; }
        public string ClientLanguageCode { get; set; }
        public string UserAgent { get; set; }
        public string ClientVersion { get; set; }
        public string CompanyKey { get; set; }
        public string CompanyName { get; set; }
        public string Market { get; set; }
    }
}
