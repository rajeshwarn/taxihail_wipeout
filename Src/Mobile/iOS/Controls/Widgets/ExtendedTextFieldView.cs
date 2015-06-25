using System;
using UIKit;
using Foundation;
using CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("ExtendedTextFieldView")]
    public class ExtendedTextFieldView : UITextField
    {
        public ExtendedTextFieldView (IntPtr handle) : base (handle)
        {

        }

        public ExtendedTextFieldView ()
        {

        }

        public ExtendedTextFieldView (CGRect frame) : base (frame)
        {

        }

        public event EventHandler BackButtonPressed;


        [Preserve]
        [Export("keyboardInputShouldDelete:")]
        private bool KeyboardInputShouldDelete(UITextField textField)
        {
            if(BackButtonPressed != null)
            {
                BackButtonPressed(this, new EventArgs());
            }
            return true;
        }
    }
}

