using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class TextFieldWithArrow: TextField
    {
        UIImageView _rightArrow;  
        public TextFieldWithArrow(IntPtr handle) : base(handle)
        {
        }
        
        public TextFieldWithArrow(RectangleF rect) : base( rect )
        {
        }

        public override void WillMoveToSuperview (UIView newsuper)
        {
            base.WillMoveToSuperview (newsuper);
            if (_rightArrow == null) {
                _rightArrow = new UIImageView(new RectangleF(this.Frame.Width - 25, this.Frame.Height/2 - 7,9, 13));
                _rightArrow.Image = UIImage.FromFile("Assets/Cells/rightArrow.png");
                this.AddSubview(_rightArrow);
            }
        }
    }
}

