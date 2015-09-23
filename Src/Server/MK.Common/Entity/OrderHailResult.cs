
using System;

namespace apcurium.MK.Common.Entity
{
    public class OrderHailResult
    {
        public Guid TaxiHailOrderId { get; set; }

        public OrderKey OrderKey { get; set; }

        public VehicleCandidate[] VehicleCandidates { get; set; }
    }
}
