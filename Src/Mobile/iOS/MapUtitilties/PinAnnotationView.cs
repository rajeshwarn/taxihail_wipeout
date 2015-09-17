using System;
using CoreGraphics;
using Foundation;
using MapKit;
using UIKit;
using apcurium.MK.Booking.Mobile.Client.Style;
using System.Threading;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Client.MapUtitilties
{
	public class PinAnnotationView : MKAnnotationView
    {   
        private UILabel _lblVehicleNumber;

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
            CreateMedailonView();
		}

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            var ann = (AddressAnnotation)Annotation;

            ann.HideMedaillonsCommand.ExecuteIfPossible();

            var addressType = ann.AddressType;
            if (addressType == AddressAnnotationType.NearbyTaxi
                || addressType == AddressAnnotationType.Taxi)
            {
                _lblVehicleNumber.Hidden = !_lblVehicleNumber.Hidden; 
            }
        }

        public void HideMedaillon()
        {
            _lblVehicleNumber.Hidden = true; 
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

        private void CreateMedailonView()
        {
            _lblVehicleNumber = new UILabel(new CGRect(0, -23, Image.Size.Width, 20));
            _lblVehicleNumber.BackgroundColor = UIColor.DarkGray;
            _lblVehicleNumber.TextColor = UIColor.White;
            _lblVehicleNumber.TextAlignment = UITextAlignment.Center;
            _lblVehicleNumber.Font = UIFont.FromName (FontName.HelveticaNeueRegular, 30 / 2);
            _lblVehicleNumber.AdjustsFontSizeToFitWidth = true;
            _lblVehicleNumber.Text = ((AddressAnnotation) Annotation).Title; 
            _lblVehicleNumber.Hidden = true;
            AddSubview(_lblVehicleNumber);
        }

		public void RefreshPinImage ()
        {
            var ann = ((AddressAnnotation)Annotation);

            Image = ann.GetImage();

            // The show vehicle number setting is handled at this level so the number can still be populated and used elsewhere
            if (ann.AddressType == AddressAnnotationType.Taxi && ann.ShowSubtitleOnPin) 
            {
                _lblVehicleNumber.Text = ((AddressAnnotation) Annotation).Subtitle;
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


