
namespace apcurium.MK.Common.Entity
{
    public class VehicleCandidate
    {
        public VehicleCandidateTypes CandidateType { get; set; }

        public string VehicleId { get; set; }

        public int ETADistance { get; set; }

        public int ETATime { get; set; }

        public string Rating { get; set; }
    }
}
