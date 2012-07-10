
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;

namespace TaxiMobileApp
{
	public class AppStyle
	{
		public AppStyle ()
		{
		}
		
				
		public  static  UIColor AcceptButtonColor{get {return UIColor.FromRGB (19, 147, 11);}}
		public  static UIColor AcceptButtonHighlightedColor{get {return UIColor.FromRGB (12, 98, 8);}}
		
		public  static UIColor CancelButtonColor{get {return UIColor.FromRGB (215, 0, 2);}}
		public  static UIColor CancelButtonHighlightedColor{get {return UIColor.FromRGB (136, 0, 2);}}
		
		public  static UIColor LightButtonColor{get {return UIColor.FromRGB (90, 90, 90);}}
		public  static UIColor LightButtonHighlightedColor{get {return UIColor.FromRGB (50,50,50);}}

		public static UIColor NavBarColor
		{
			get { return UIColor.FromRGB( 174,181,137); }
		}
	}
}

