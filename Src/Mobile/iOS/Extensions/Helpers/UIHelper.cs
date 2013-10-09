using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Extensions.Helpers
{
    public class UIHelper
    {

        
        public static bool IsRetinaDisplay { get { return UIScreen.MainScreen.Scale > 1.0; } }
        public static bool Is16by9Display { get { return (UIScreen.MainScreen.Bounds.Height / UIScreen.MainScreen.Bounds.Width) < 2; } }
        
        public static UIImage GetImageFromColor( SizeF size, UIColor color )
        {
            UIGraphics.BeginImageContextWithOptions( size, false, 0) ;
            var rect = new RectangleF(0, 0, size.Width, size.Height );
            color.SetFill();
            UIGraphics.RectFill( rect );
            var  image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();                         
            return image;           
        }

        public static bool Is4InchDisplay
        {
            get { return UIScreen.MainScreen.Bounds.Height > 500; }
        }

    }
}

