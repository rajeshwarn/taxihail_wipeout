using System;
using UIKit;
using CoreGraphics;
using Foundation;
using apcurium.MK.Booking.Mobile.Client.Helper;

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

        public FlatCheckBox (CGRect frame) : base (frame)
        {
            Initialize ();
        }
        public FlatCheckBox ()
        {
            Initialize ();
        }

        private void Initialize()
        {
            SetImage(ImageHelper.ApplyThemeTextColorToImage("unchecked.png"), UIControlState.Normal);
            SetImage(ImageHelper.ApplyThemeTextColorToImage("checked.png"), UIControlState.Selected);
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

        public void ApplyContrastBasedColor()
        {
          SetImage(ImageHelper.ApplyContrastBasedThemeTextColorToImage("unchecked.png"), UIControlState.Normal);
          SetImage(ImageHelper.ApplyContrastBasedThemeTextColorToImage("checked.png"), UIControlState.Selected);
        }
    }
}

