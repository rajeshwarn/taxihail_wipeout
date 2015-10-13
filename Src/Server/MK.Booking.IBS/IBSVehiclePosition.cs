#region

using System;

#endregion

namespace apcurium.MK.Booking.IBS
{
    public class IbsVehiclePosition
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double? CompassCourse { get; set; }

        public string VehicleNumber { get; set; }

        public DateTime PositionDate { get; set; }

        public int? FleetId { get; set; }

        public int? Eta { get; set; }

        public int? VehicleType { get; set; }
    }
}