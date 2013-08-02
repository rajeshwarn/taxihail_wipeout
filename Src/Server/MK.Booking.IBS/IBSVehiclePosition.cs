using System;

namespace apcurium.MK.Booking.IBS
{
    public class IBSVehiclePosition
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime PositionDate { get; set; }
    }
}
