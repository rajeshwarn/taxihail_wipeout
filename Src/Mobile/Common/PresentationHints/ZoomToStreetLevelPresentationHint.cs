namespace apcurium.MK.Booking.Mobile.PresentationHints
{
    public class ZoomToStreetLevelPresentationHint: ChangePresentationHint
    {
		public ZoomToStreetLevelPresentationHint()
		{
			
		}

        public ZoomToStreetLevelPresentationHint(double latitude, double longitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }


    }
}

