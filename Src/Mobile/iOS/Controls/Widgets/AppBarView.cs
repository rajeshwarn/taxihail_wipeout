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
        public const float Margin = 6;

        protected UIView Line { get; set; }
        public UIView BackgroundView { get; set;}

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
                BackgroundColor = UIColor.Black.ColorWithAlpha(0.548f)
            };

            AddSubview(Line);
        }

        public virtual void UpdateView(float bottom, float width)
        {
            Line.Frame = new RectangleF(0, 0, Frame.Width, UIHelper.OnePixel);
        }
    }
}

