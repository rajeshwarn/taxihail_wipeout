using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.MapDataProvider.Resources;

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
