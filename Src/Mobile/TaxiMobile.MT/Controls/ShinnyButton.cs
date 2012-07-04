using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace TaxiMobile.Controls
{
	public class ShinnyButton
	{


		private CAGradientLayer _shineLayer;
		private CALayer _highlightLayer;

		public ShinnyButton (UIButton oAddressBtn)
		{
			
			
// Create a gradient for the background.
			CAGradientLayer oGradient = new CAGradientLayer ();
			oGradient.Frame = oAddressBtn.Bounds;
			oGradient.Colors = new CGColor[] { UIColor.FromRGB (107, 182, 112).CGColor, UIColor.FromRGB (19, 142, 12).CGColor };
			
			
			
// Assign gradient to the button.
			oAddressBtn.Layer.MasksToBounds = true;
			oAddressBtn.Layer.AddSublayer (oGradient);
			oAddressBtn.Layer.CornerRadius = 10;
			oAddressBtn.Layer.BorderColor = UIColor.DarkGray.CGColor;
			oAddressBtn.Layer.BorderWidth = 2;
			
// Set the button's title. Alignment and font have to be set here to make it work.
			oAddressBtn.VerticalAlignment = UIControlContentVerticalAlignment.Center;
			oAddressBtn.Font = UIFont.FromName ("Helvetica", 12);
			oAddressBtn.SetTitleColor (UIColor.White, UIControlState.Normal);
			
			oAddressBtn.SetTitle ("This is a test", UIControlState.Normal);
			
			
//			InitBorder (oAddressBtn);
//			AddShinyLayer (button);
		}

		private void InitBorder (UIButton button)
		{
			button.Layer.CornerRadius = 8.0f;
			
			button.Layer.MasksToBounds = true;
			
			button.Layer.BorderWidth = 1;
			
			button.Layer.BorderColor = UIColor.FromWhiteAlpha (0.5f, 0.2f).CGColor;
		}


		private void AddShinyLayer (UIButton button)
		{
			
			_shineLayer = new CAGradientLayer ();
			
			
			_shineLayer.Frame = button.Layer.Bounds;
			
			_shineLayer.Colors = new CGColor[] { UIColor.FromWhiteAlpha (1.0f, 0.4f).CGColor, UIColor.FromWhiteAlpha (1.0f, 0.2f).CGColor, UIColor.FromWhiteAlpha (0.75f, 0.2f).CGColor, UIColor.FromWhiteAlpha (0.4f, 0.2f).CGColor, UIColor.FromWhiteAlpha (1.0f, 0.4f).CGColor };
			
			
			_shineLayer.Locations = new NSNumber[] { NSNumber.FromFloat (0.0f), NSNumber.FromFloat (0.0f), NSNumber.FromFloat (0.5f), NSNumber.FromFloat (0.5f), NSNumber.FromFloat (0.8f), NSNumber.FromFloat (1.0f) };
			button.Layer.AddSublayer (_shineLayer);
		}

		private void AddHighlightLayer (UIButton button)
		{
			_highlightLayer = new CALayer ();
			
//    _highlightLayer.BackgroundColor = UIColor.FromRGBA( 0.25f, 0.25f , 0.25f , 0.75).CGColor;
			
			_highlightLayer.Frame = button.Layer.Bounds;
			
			_highlightLayer.Hidden = true;
			
			
		}
		
	}
}

