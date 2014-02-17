using apcurium.MK.Booking.Mobile.Client.Controls;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using apcurium.MK.Booking.Mobile.Client.Views;

namespace apcurium.MK.Booking.Mobile.Client.MapUtitilties
{
	public class AddressMapDelegate : MKMapViewDelegate
	{
		private readonly bool _regionMovedActivated;

		public AddressMapDelegate (bool regionMovedActivated = true )
		{
			_regionMovedActivated = regionMovedActivated;
		}
        	

        public override void RegionChanged (MKMapView mapView, bool animated)
        {
            if (_regionMovedActivated)
            {
                //TODO remove when status is migrated to new map
                ((TouchMap)mapView).OnRegionChanged();
            }
        }

	}
}

