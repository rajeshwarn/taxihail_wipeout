
namespace apcurium.MK.Common.Entity
{
    public class OrderHailResult
    {
        public OrderHailResult()
        {
            VehicleCandidates = new VehicleCandidate[0];
        }

        public OrderKey OrderKey { get; set; }

        public VehicleCandidate[] VehicleCandidates { get; set; }
    }
}
