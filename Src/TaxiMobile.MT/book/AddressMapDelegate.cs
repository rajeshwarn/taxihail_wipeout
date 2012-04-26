using MonoTouch.Foundation;
using MonoTouch.MapKit;

namespace TaxiMobile.Book
{
	public class AddressMapDelegate : MKMapViewDelegate
	{
		public AddressMapDelegate ()
		{
			
		}
		
		
		
		
		public override MKAnnotationView GetViewForAnnotation (MKMapView mapView, NSObject annotation)
		{
			var ann = annotation as AddressAnnotation;
			
			if (ann == null) {
				return null;
			
			} else {
//				if ( ann.AddressType == AddressAnnotationType.Taxi )
//				{
//					TaxiAnnotationView anv = mapView.DequeueReusableAnnotation ( "thislocation" ) as TaxiAnnotationView;
//				if ( anv == null )
//				{
//					anv = new TaxiAnnotationView( ann ,  "thislocation");
//					
//				}
//				else
//				{					
//					anv.Annotation = ann;										
//				}
//																
//				return anv;
//					
//				}				
//				else{
				MKPinAnnotationView anv = mapView.DequeueReusableAnnotation ("thislocation") as MKPinAnnotationView;
				if (anv == null) {
					anv = new MKPinAnnotationView (annotation, "thislocation");
					
				} else {
					anv.Annotation = ann;
				}
				anv.AnimatesDrop = false;
				
				anv.CanShowCallout = true;
				
				if (ann.AddressType == AddressAnnotationType.Pickup) {
					anv.PinColor = MKPinAnnotationColor.Green;
				} else if (ann.AddressType == AddressAnnotationType.Destination) {
					anv.PinColor = MKPinAnnotationColor.Red;
				} else if (ann.AddressType == AddressAnnotationType.Taxi) {
					anv.PinColor = MKPinAnnotationColor.Purple;
									anv.AnimatesDrop = false;
				}
				
				return anv;
			}
		}
	}
}

