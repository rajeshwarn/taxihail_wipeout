using System;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.MapUtitilties;

namespace MonoTouch.MapKit
{
    public static class MKMapViewHelper
    {
        public static MKAnnotationView GetViewForAnnotation (MKMapView mapView, NSObject annotation)
        {
            var ann = annotation as AddressAnnotation;

            if (ann == null) 
            {
                return null;
            }

			var anv = mapView.DequeueReusableAnnotation (ann.AddressType.ToString()) as PinAnnotationView;
            if (anv == null) 
            {
				anv = new PinAnnotationView (ann, ann.AddressType.ToString());
            } 
            else 
            {
                anv.Annotation = ann;
                anv.RefreshPinImage();
            }

            anv.CanShowCallout = ann.AddressType != AddressAnnotationType.Taxi;

            return anv;
        }
    }
}
