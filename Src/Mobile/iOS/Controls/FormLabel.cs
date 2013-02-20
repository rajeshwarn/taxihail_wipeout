using System;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client
{
    [Register("FormLabel")]
    public class FormLabel : UILabel
    {
        public FormLabel(IntPtr handle) : base(handle)
        {
            Initialize();
        }
        
        public FormLabel(RectangleF rect) : base( rect )
        {
            Initialize();
        }
        
        private void Initialize() {
        
            Font = AppStyle.NormalTextFont;
            TextColor = UIColor.FromRGB(100,100,100);
            ShadowColor = UIColor.FromRGBA(255, 255, 255, 191);
            ShadowOffset = new SizeF(0,1);

        }
       
    }
}

