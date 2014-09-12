using System;
using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Localization;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class UITextViewExtensions
    {
        public static void ShowCloseButtonOnKeyboard(this UITextView text, Action onClosePressed = null)
        {
            text.InputAccessoryView = new UIView { Frame = new RectangleF(0, 0, 320, 44), BackgroundColor = UIColor.FromRGB(251, 253, 253) };

            var closeButton = new FlatButton();
            var closeButtonText = Localize.GetValue ("OkButtonText");
            closeButton.SetTitle(closeButtonText, UIControlState.Normal);           
            closeButton.TranslatesAutoresizingMaskIntoConstraints = false;
            FlatButtonStyle.Green.ApplyTo(closeButton);
            text.InputAccessoryView.AddSubview(closeButton);

            var widthOfText = closeButton.GetSizeThatFits (closeButtonText, closeButton.Font).Width;
            var totalTextPaddingInButton = 30f;

            closeButton.AddConstraints(new [] {
                NSLayoutConstraint.Create(closeButton, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, 36f),
                NSLayoutConstraint.Create(closeButton, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1, widthOfText + totalTextPaddingInButton)
            });

            text.InputAccessoryView.AddConstraints(new [] {
                NSLayoutConstraint.Create(closeButton, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, text.InputAccessoryView, NSLayoutAttribute.Trailing, 1, -8f),
                NSLayoutConstraint.Create(closeButton, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, text.InputAccessoryView, NSLayoutAttribute.CenterY, 1, 0),
            });

            if (onClosePressed != null)
            {
                closeButton.TouchUpInside += (sender, e) =>
                {
                    text.ResignFirstResponder();
                    onClosePressed();
                };
            }
            else
            {
                closeButton.TouchUpInside += (sender, e) => text.ResignFirstResponder();
            }
        }
    }
}

