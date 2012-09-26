using System;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.MapUtilities
{
	public class PinAnnotationView : MKAnnotationView
	{
	
		
		[Export( "initWithCoder:" )]
		public PinAnnotationView ( NSCoder coder ) : base( coder )
		{
			
		}

		public PinAnnotationView ( IntPtr ptr ) : base( ptr )
		{
			
		}

		public PinAnnotationView ( AddressAnnotation annotation, string id ) : base( annotation, id )
		{
			Annotation = annotation;
			RefreshPinImage();
		}


		public void SetPinImage( MKPinAnnotationColor color )
		{


		}
			
		public override NSObject Annotation
		{
			get { return base.Annotation; }
			set
			{
//				if ( value == null && base.Annotation != null )
//				{
//					base.Annotation.Dispose (  );					
//					return;
//				}
				
				base.Annotation = value;
				if( value != null )
				{
					RefreshPinImage();
				}
			}
		}

		public void RefreshPinImage()
		{
			switch( ((AddressAnnotation)Annotation).AddressType )
			{
			case AddressAnnotationType.Destination:
				Image = UIImage.FromFile( "Assets/pin_red.png" );
				break;
			case AddressAnnotationType.Taxi:
				Image = UIImage.FromFile( "Assets/taxi-label.png" );
			break;
			case AddressAnnotationType.Pickup:
			default:
				Image = UIImage.FromFile( "Assets/pin_green.png" );
				break;
			}
			CenterOffset = new PointF( 0, -Image.Size.Height + 7f  );
		}
		
	}
}


