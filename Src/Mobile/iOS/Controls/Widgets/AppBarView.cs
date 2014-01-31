using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("AppBarView")]
    public class AppBarView : UIView
    {
        protected UIView Line { get; set; }

        public AppBarView (IntPtr ptr):base(ptr)
        {
            Ctor ();            
        }

        public AppBarView ()
        {
            Ctor ();
        }

        void Ctor ()
        {
            BackgroundColor = UIColor.White;

            Line = new UIView()
            {
                Frame = new RectangleF(0, 0, Frame.Width, UIHelper.OnePixel),
                BackgroundColor = UIColor.FromRGB(140, 140, 140)
            };

            AddSubview(Line);
        }
    }
}

