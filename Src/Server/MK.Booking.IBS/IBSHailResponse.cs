
namespace apcurium.MK.Booking.IBS
{
    public class IbsHailResponse
    {
        public IbsOrderKey OrderKey { get; set; }

        public IbsVehicleCandidate[] VehicleCandidates { get; set; }
    }
}
