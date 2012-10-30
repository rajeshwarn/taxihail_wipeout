using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Common.Entity
{
    public class Rate
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public decimal FlatRate { get; set; }
        public double DistanceMultiplicator { get; set; }
        public double TimeAdjustmentFactor { get; set; }
        public decimal PricePerPassenger { get; set; }
        public int DaysOfTheWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Type { get; set; }
    }
}
