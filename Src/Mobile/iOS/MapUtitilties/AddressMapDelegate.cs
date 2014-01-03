using apcurium.MK.Booking.Mobile.Client.Controls;
using MonoTouch.Foundation;
using MonoTouch.MapKit;

namespace apcurium.MK.Booking.Mobile.Client.MapUtitilties
{
	public class AddressMapDelegate : MKMapViewDelegate
	{
		private readonly bool _regionMovedActivated;

		public AddressMapDelegate (bool regionMovedActivated = true )
		{
			_regionMovedActivated = regionMovedActivated;
		}

		public override MKAnnotationView GetViewForAnnotation (MKMapView mapView, NSObject annotation)
		{
			var ann = annotation as AddressAnnotation;
			
			if (ann == null) 
			{
				return null;
			}
		    var anv = mapView.DequeueReusableAnnotation ("thislocation") as PinAnnotationView;
		    if (anv == null) 
		    {
		        anv = new PinAnnotationView (ann, "thislocation");	
		    } 
		    else 
		    {
		        anv.Annotation = ann;
		        anv.RefreshPinImage();
		    }

		    anv.CanShowCallout = ann.AddressType != AddressAnnotationType.Taxi;

		    return anv;
		}

		public override void RegionChanged (MKMapView mapView, bool animated)
		{
			if( _regionMovedActivated )
			{
				((TouchMap)mapView).OnRegionChanged();
			}
		}

	}
}

