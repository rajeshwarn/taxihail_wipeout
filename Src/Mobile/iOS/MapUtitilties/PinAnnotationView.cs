using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.MapUtitilties
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
			
		public override sealed NSObject Annotation
		{
			get { return base.Annotation; }
			set
			{
				base.Annotation = value;
				if( value != null )
				{
					RefreshPinImage();
				}
			}
		}

		public void RefreshPinImage ()
        {
            var ann = ((AddressAnnotation)Annotation);
            Image = UIImage.FromFile (ann.GetImageFilename());

            if (ann.AddressType == AddressAnnotationType.Taxi) {
                var lblVehicleNumber = new UILabel(new RectangleF(0,0,Image.Size.Width, 16));
                lblVehicleNumber.BackgroundColor = UIColor.Clear;
                lblVehicleNumber.TextColor = UIColor.Black;
                lblVehicleNumber.TextAlignment = UITextAlignment.Center;
                lblVehicleNumber.Font = AppStyle.GetBoldFont(12);
                lblVehicleNumber.Text = ((AddressAnnotation)Annotation).Subtitle;
                AddSubview( lblVehicleNumber );
            }
			CenterOffset = new PointF( 0, -Image.Size.Height/2);
		}
		
	}
}


