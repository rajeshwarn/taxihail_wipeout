using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Common.Entity
{
    public class ManualRideLinqDetails
    {
        public Guid OrderId { get; set; }
        public Guid AccountId { get; set; }
        public string RideLinqId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
