using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.ViewModels.Map
{
	public class TaxiLocation
	{
		public double? Latitude { get; set; }

        public double? Longitude { get; set; }
        public double? CompassCourse { get; set; }
        
		public string VehicleNumber { get; set; }

        public string Market { get; set; }

		public ServiceType ServiceType { get; set; }
	}
}