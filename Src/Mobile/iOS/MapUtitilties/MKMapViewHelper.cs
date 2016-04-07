using apcurium.MK.Booking.Mobile.Client.MapUtitilties;

namespace MapKit
{
    public static class MKMapViewHelper
    {
        public static MKAnnotationView GetViewForAnnotation (MKMapView mapView, IMKAnnotation annotation)
        {
            var ann = annotation as AddressAnnotation;

            if (ann == null) 
            {
                return null;
            }

			var anv = mapView.DequeueReusableAnnotation (ann.AddressType.ToString()) as PinAnnotationView;
            if (anv == null) 
            {
				anv = new PinAnnotationView (ann, ann.AddressType.ToString())
					{
						ShowMedallionOnTap = ann.ShowMedallionOnTap
					};
            } 
            else 
            {
                anv.Annotation = ann;
                anv.RefreshPinImage();
				anv.ShowMedallionOnTap = ann.ShowMedallionOnTap;
            }

            anv.Enabled = false; //disables the popup when you tap the annotation
            anv.CanShowCallout = ann.AddressType != AddressAnnotationType.Taxi;
            if (!ann.ShowMedallionOnStart)
            {
                anv.HideMedaillon();
            }

            return anv;
        }
    }
}
