using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Localization;
using System.Reactive.Linq;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class UITextFieldExtensions
    {
        public static void TapAnywhereToClose(this UITextField text, Func<UIView> owner)
        {
            var giantInvisibleButton = new UIButton();

            text.EditingDidBegin += (sender, e) => {
                var o = owner();
                o.AddSubview(giantInvisibleButton);
                giantInvisibleButton.Frame = o.Frame.Copy();
            };
            text.EditingDidEnd += (sender, e) => {           
                giantInvisibleButton.RemoveFromSuperview();
            };

            giantInvisibleButton.TouchDown += (sender, e) => {
                text.ResignFirstResponderOnSubviews();
            };
        }

        public static void SetLeftPadding( this UITextField text, float padding )
        {
            var btn = new UIButton(new RectangleF(0, 0, padding, text.Bounds.Height));
            text.LeftView = btn;
            btn.TouchUpInside += (sender, e) => text.BecomeFirstResponder();
            text.LeftViewMode = UITextFieldViewMode.Always;
        }

        public static void SetRightPadding( this UITextField text, float padding )
        {
            var btn = new UIButton(new RectangleF(0, 0, padding, text.Bounds.Height));
            text.RightView = btn;
            btn.TouchUpInside += (sender, e) => text.BecomeFirstResponder();
            text.RightViewMode = UITextFieldViewMode.Always;
        }

        public static void ShowCloseButtonOnKeyboard(this UITextField text, Action onClosePressed = null)
        {
            text.InputAccessoryView = new UIView { Frame = new RectangleF(0, 0, 320, 44), BackgroundColor = UIColor.FromRGB(251, 253, 253) };
            var closeButton = new AppBarLabelButton("");
            text.InputAccessoryView.AddSubview(closeButton);

            closeButton.SetTitle(Localize.GetValue("OkButtonText"), UIControlState.Normal);           
            closeButton.TranslatesAutoresizingMaskIntoConstraints = false;

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

