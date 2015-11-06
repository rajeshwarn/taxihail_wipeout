
namespace apcurium.MK.Booking.IBS
{
    public class IbsResponse
    {
        public IbsOrderKey OrderKey { get; set; }

        public IbsVehicleCandidate[] VehicleCandidates { get; set; }
    }
}
