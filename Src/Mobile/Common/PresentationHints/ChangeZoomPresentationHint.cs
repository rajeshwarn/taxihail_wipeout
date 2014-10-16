using apcurium.MK.Booking.Maps.Geo;

namespace apcurium.MK.Booking.Mobile.PresentationHints
{
	public class ChangeZoomPresentationHint : ChangePresentationHint
	{
		public ChangeZoomPresentationHint()
		{

		}

		public ChangeZoomPresentationHint(MapBounds bounds)
		{
			Bounds = bounds;
		}

		public MapBounds Bounds { get; set; }
	}
}
