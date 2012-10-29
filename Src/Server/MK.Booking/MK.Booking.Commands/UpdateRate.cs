using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class UpdateRate : ICommand
    {
        public UpdateRate()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public Guid CompanyId { get; set; }
        public Guid RateId { get; set; }
        public string Name { get; set; }
        public decimal FlatRate { get; set; }
        public double DistanceMultiplicator { get; set; }
        public double TimeAdjustmentFactor { get; set; }
        public decimal PricePerPassenger { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DayOfTheWeek DaysOfTheWeek { get; set; }
    }
}
