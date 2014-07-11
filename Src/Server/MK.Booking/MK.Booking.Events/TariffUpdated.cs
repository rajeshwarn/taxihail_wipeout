#region

using System;
using apcurium.MK.Common.Entity;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Events
{
    public class TariffUpdated : VersionedEvent
    {
        public Guid TariffId { get; set; }
        public string Name { get; set; }
        public decimal FlatRate { get; set; }
        public double KilometricRate { get; set; }
        public double PerMinuteRate { get; set; }
        public double MarginOfError { get; set; }
        public double KilometerIncluded { get; set; }
        public DayOfTheWeek DaysOfTheWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}