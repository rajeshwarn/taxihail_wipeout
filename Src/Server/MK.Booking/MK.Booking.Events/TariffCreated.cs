using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Events
{
    public class TariffCreated : VersionedEvent
    {
        public Guid TariffId { get; set; }
        public string Name { get; set; }
        public decimal FlatRate { get; set; }
        public double KilometricRate { get; set; }
        public double MarginOfError { get; set; }
        public decimal PassengerRate { get; set; }
        public DayOfTheWeek DaysOfTheWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TariffType Type { get; set; }
    }
}
