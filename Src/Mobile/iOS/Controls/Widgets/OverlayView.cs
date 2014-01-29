using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("OverlayView")]
    public class OverlayView : UIView
    {
        public OverlayView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        public OverlayView(RectangleF frame) : base(frame)
        {
            Initialize();
        }

        public OverlayView() : base()
        {
            Initialize();
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.White.ColorWithAlpha(0.9f);
        }
    }
}

