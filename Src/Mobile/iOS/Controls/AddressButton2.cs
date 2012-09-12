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

        public string TextLine1
        {
            get { return Title(UIControlState.Normal);}
            set { SetTitle(value, UIControlState.Normal);}
        }

        public string TextLine2
        {
            get;
            set;
        }
    }
}


  