using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MapKit;
using MonoTouch.CoreLocation;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Client.Controls;

namespace apcurium.MK.Booking.Mobile.Client.MapUtilities
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
			
			} 
			else 
			{
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
				PinAnnotationView anv = mapView.DequeueReusableAnnotation ("thislocation") as PinAnnotationView;
				if (anv == null) 
				{
					anv = new PinAnnotationView (ann, "thislocation");	
				} 
				else 
				{
					anv.Annotation = ann;
					anv.RefreshPinImage();
				}

				//anv.AnimatesDrop = false;
				
				anv.CanShowCallout = true;

				
//				if (ann.AddressType == AddressAnnotationType.Pickup) 
//				{
//					anv.SetPinColor(MKPinAnnotationColor.Green);
//				} 
//				else if (ann.AddressType == AddressAnnotationType.Destination) 
//				{
//					anv.SetPinColor(MKPinAnnotationColor.Red);
//				} 
//				else if (ann.AddressType == AddressAnnotationType.Taxi) 
//				{
//					anv.SetPinColor(MKPinAnnotationColor.Purple);
//				}
				return anv;
			}
		}

		public override void RegionChanged (MKMapView mapView, bool animated)
		{
			((TouchMap)mapView).OnRegionChanged();
		}
	}
}

