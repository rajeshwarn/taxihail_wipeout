using apcurium.MK.Booking.IBS;

namespace apcurium.MK.Booking.Data
{
    public class IbsOrderParams
    {
        public int? IbsChargeTypeId { get; set; }

        public IbsAddress IbsPickupAddress { get; set; }

        public IbsAddress IbsDropOffAddress { get; set; }

        public int? CustomerNumber { get; set; }

        public int DefaultVehicleTypeId { get; set; }

        public int? ProviderId { get; set; }
    }
}
