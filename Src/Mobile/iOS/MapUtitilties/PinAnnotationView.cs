using System;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Common.Enumeration;
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
            CreateOrUpdateMedaillonView(annotation.Title, annotation.Market, hidden:true);
		}

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            var ann = (AddressAnnotation)Annotation;

            // We should only allow changes to the visibility of the medallion for nearby taxi.
            if (ann.AddressType != AddressAnnotationType.NearbyTaxi)
            {
                return;
            }

            ann.HideMedaillonsCommand.ExecuteIfPossible();

            _lblVehicleNumber.Hidden = !_lblVehicleNumber.Hidden; 
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
                if (value == null)
                {
                    return;
                }

                RefreshPinImage();
            }
        }

	    public void RefreshPinImage()
	    {
	        var ann = ((AddressAnnotation) Annotation);
	        var degrees = ann.Degrees;

	        Image = ann.GetImage();

	        // The show vehicle number setting is handled at this level so the number can still be populated and used elsewhere
            if (ann.AddressType == AddressAnnotationType.Taxi || ann.AddressType == AddressAnnotationType.NearbyTaxi)
	        {
	            var addressAnnotation = (AddressAnnotation) Annotation;
                var medallion = ann.ShowSubtitleOnPin 
                    ? addressAnnotation.Subtitle 
                    : addressAnnotation.Title;
                
                CreateOrUpdateMedaillonView(medallion, addressAnnotation.Market, hidden: !ann.ShowMedallionOnStart);
	        }

	        if (degrees != 0)
	        {
	            CenterOffset = new CGPoint(0, 0);
	        }
	        else
	        {
	            CenterOffset = new CGPoint(0, -Image.Size.Height/2);
	            if (ann.AddressType == AddressAnnotationType.Destination
	                || ann.AddressType == AddressAnnotationType.Pickup)
	            {
	                CenterOffset = new CGPoint(0, -Image.Size.Height/2 + 2);
	            }
	        }
	    }

	    private void CreateOrUpdateMedaillonView(string text, string market, bool hidden)
        {
            if (_lblVehicleNumber == null)
            {
                var lblVehicleNumber = new UILabel(new CGRect(0, -23, Image.Size.Width, 20));
                lblVehicleNumber.TextColor = UIColor.White;
                lblVehicleNumber.TextAlignment = UITextAlignment.Center;
                lblVehicleNumber.Font = UIFont.FromName(FontName.HelveticaNeueRegular, 30 / 2);
                lblVehicleNumber.AdjustsFontSizeToFitWidth = true;
                AddSubview(lblVehicleNumber);
                _lblVehicleNumber = lblVehicleNumber;
            }

            _lblVehicleNumber.BackgroundColor = GetMedaillonBackgroundColor(market);
            _lblVehicleNumber.Text = text;
            _lblVehicleNumber.Hidden = hidden;
        }

	    private UIColor GetMedaillonBackgroundColor(string market)
	    {
	        if (!market.HasValue())
	        {
                return UIColor.DarkGray;
	        }

	        switch (market.ToLower())
	        {
	            case AssignedVehicleMarkets.NYC:
                    return UIColor.FromRGB(243, 177, 20); // Yellow
                case AssignedVehicleMarkets.NYSHL:
                    return UIColor.FromRGB(92, 127, 18); // Green
                default:
	                return UIColor.DarkGray;
	        }
	    }
	}
}


