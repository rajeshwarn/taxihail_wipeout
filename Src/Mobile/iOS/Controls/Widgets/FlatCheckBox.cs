using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("FlatCheckBox")]
    public class FlatCheckBox : UIButton
    {
        public event EventHandler CheckValueChanged;

        public FlatCheckBox (IntPtr handle) : base (handle)
        {
            Initialize ();
        }

        public FlatCheckBox (RectangleF frame) : base (frame)
        {
            Initialize ();
        }
        public FlatCheckBox ()
        {
            Initialize ();
        }

        private void Initialize()
        {
            SetImage(UIImage.FromFile("unchecked.png"), UIControlState.Normal);
            SetImage(UIImage.FromFile("checked.png"), UIControlState.Selected);
        }

        public override bool BeginTracking(UITouch uitouch, UIEvent uievent)
        {
            Selected = !Selected;
            var handler = CheckValueChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
            return true;
        }
    }
}

