using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
    [Register ("AddressButton")]
    public class AddressButton : GradientButton 
    {

        public AddressButton(IntPtr handle) : base(  handle )
        {
        }
        
        
        public AddressButton(RectangleF rect, float cornerRadius, Style.ButtonStyle buttonStyle, string title, UIFont titleFont, string image = null) : base ( rect , cornerRadius, buttonStyle, title, titleFont, image )
        {

        }
    }
}


  