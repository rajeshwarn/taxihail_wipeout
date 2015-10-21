using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.IBS
{
    public class IbsVehicleCandidate
    {
        public VehicleCandidateTypes CandidateType { get; set; }

        public string VehicleId { get; set; }

        public int ETADistance { get; set; }

        public int ETATime { get; set; }

        public string Rating { get; set; }
    }
}
