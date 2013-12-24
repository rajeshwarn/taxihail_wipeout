#region

using System;

#endregion

namespace apcurium.MK.Booking.IBS
{
    public class IbsVehiclePosition
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime PositionDate { get; set; }
    }
}