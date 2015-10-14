using System;

namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class AvailableVehicle : BaseDto
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double CompassCourse { get; set; }

		[Obsolete]
        public double VehicleNumber { get; set; }

		public string VehicleName { get; set; }

        public string LogoName { get; set; }

        public int? FleetId { get; set; }


        public int? Eta { get; set; }

        public int? VehicleType { get; set; }

        public string Market { get; set; }
    }

    public class AvailableVehicleCluster : AvailableVehicle
    {
    }
}