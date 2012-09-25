using System;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.MapUtilities
{
	public class PinAnnotationView : MKPinAnnotationView
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
			ContentMode = UIViewContentMode.TopLeft;
		}

		 
		public override MKPinAnnotationColor PinColor {
			get {
				return base.PinColor;
			}
			set {
				base.PinColor = value;
				SetPinImage();
			}
		}
			
		public override NSObject Annotation
		{
			get { return base.Annotation; }
			set
			{
				if ( value == null )
				{
					base.Annotation.Dispose (  );					
					return;
				}
				
				base.Annotation = value;										
			}
		}

		private void SetPinImage()
		{
			switch( PinColor )
			{
			case MKPinAnnotationColor.Red:
				Image = UIImage.FromFile( "Assets/pin_red.png" );
				break;
			case MKPinAnnotationColor.Green:
			default:
				Image = UIImage.FromFile( "Assets/pin_green.png" );
				break;
			}

		}
		
		
	}
}


