using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Data
{
    public class IBSOrderResult
    {
        public IBSOrderResult()
        {
            VehicleCandidates = new VehicleCandidate[0];
        }

        public OrderKey OrderKey { get; set; }

        public VehicleCandidate[] VehicleCandidates { get; set; }
    }
}
