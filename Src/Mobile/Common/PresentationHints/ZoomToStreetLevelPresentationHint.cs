using apcurium.MK.Booking.Maps.Geo;

namespace apcurium.MK.Booking.Mobile.PresentationHints
{
    public class ZoomToStreetLevelPresentationHint: ChangePresentationHint
    {
		public ZoomToStreetLevelPresentationHint()
		{
			
		}

		public ZoomToStreetLevelPresentationHint(double latitude, double longitude, MapBounds availableVehiclesZoom = null)
        {
            Longitude = longitude;
            Latitude = latitude;
			AvailableVehiclesZoom = availableVehiclesZoom;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
		public MapBounds AvailableVehiclesZoom { get; set; }
    }
}

