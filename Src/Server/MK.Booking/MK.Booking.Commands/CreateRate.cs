using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Commands
{
    public class CreateRate : ICommand
    {
        public CreateRate()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }

        public Guid RateId { get; set; }

        public decimal FlatRate { get; set; }
        public double DistanceMultiplicator { get; set; }
        public double TimeAdjustmentFactor { get; set; }
        public decimal PricePerPassenger { get; set; }
        public DayOfTheWeek DaysOfTheWeek { get; set; }
    }
}
