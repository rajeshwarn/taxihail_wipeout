using apcurium.MK.Booking.Maps.Geo;

namespace apcurium.MK.Booking.Mobile.PresentationHints
{
    public class ZoomToStreetLevelPresentationHint: ChangePresentationHint
    {
		public ZoomToStreetLevelPresentationHint()
		{
			
		}

		public ZoomToStreetLevelPresentationHint(double latitude, double longitude, MapBounds bounds = null)
        {
            Longitude = longitude;
            Latitude = latitude;
			Bounds = bounds;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
		public MapBounds Bounds { get; set; }
    }
}

