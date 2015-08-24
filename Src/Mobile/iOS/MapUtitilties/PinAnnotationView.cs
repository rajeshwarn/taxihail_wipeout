using System;
using CoreGraphics;
using Foundation;
using MapKit;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Threading;

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

        public override IMKAnnotation Annotation
        {
            get
            {
                #if DEBUG
                //problem of getting UIKit Consistency error
                if (Thread.CurrentThread.IsBackground) 
                {
                    return null;
                }
                #endif
                return base.Annotation;
            }
            set
            {
                base.Annotation = value;
                if (value != null)
                {
                    RefreshPinImage();
                }
            }
        }

		public void RefreshPinImage ()
        {
            var ann = ((AddressAnnotation)Annotation);
            Image = ann.GetImage();

            // The show vehicle number setting is handled at this level so the number can still be populated and used elsewhere
            if (ann.AddressType == AddressAnnotationType.Taxi && ann.ShowSubtitleOnPin) 
            {
                var lblVehicleNumber = new UILabel (new CGRect (0, -23, Image.Size.Width, 20));
                lblVehicleNumber.BackgroundColor = UIColor.DarkGray;
                lblVehicleNumber.TextColor = UIColor.White;
                lblVehicleNumber.TextAlignment = UITextAlignment.Center;
                lblVehicleNumber.Font = UIFont.FromName (FontName.HelveticaNeueRegular, 30 / 2);
                lblVehicleNumber.AdjustsFontSizeToFitWidth = true;
                lblVehicleNumber.Text = ((AddressAnnotation) Annotation).Subtitle;

                AddSubview (lblVehicleNumber);
            }

            CenterOffset = new CGPoint (0, -Image.Size.Height / 2);
            if (ann.AddressType == AddressAnnotationType.Destination ||
               ann.AddressType == AddressAnnotationType.Pickup)
            {
                CenterOffset = new CGPoint(0, -Image.Size.Height / 2 + 2);
            }
             
		}
	}
}


