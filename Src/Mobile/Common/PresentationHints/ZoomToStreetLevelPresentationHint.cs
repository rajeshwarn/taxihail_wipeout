using apcurium.MK.Booking.Maps.Geo;

namespace apcurium.MK.Booking.Mobile.PresentationHints
{
    public class ZoomToStreetLevelPresentationHint: ChangePresentationHint
    {
		public ZoomToStreetLevelPresentationHint()
		{
			
		}

		public ZoomToStreetLevelPresentationHint(double latitude, double longitude, bool initialZoom = false)
        {
            Longitude = longitude;
            Latitude = latitude;
			InitialZoom = initialZoom;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
		public bool InitialZoom { get; set; }
    }
}

