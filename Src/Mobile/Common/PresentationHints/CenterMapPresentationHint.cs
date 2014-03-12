namespace apcurium.MK.Booking.Mobile.PresentationHints
{
	public class CenterMapPresentationHint: ChangePresentationHint
	{
		public CenterMapPresentationHint(double latitude, double longitude)
		{
			Longitude = longitude;
			Latitude = latitude;
		}

		public double Latitude { get; private set; }
		public double Longitude { get; private set; }
	}
}

