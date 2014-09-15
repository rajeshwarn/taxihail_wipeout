using System;
using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Localization;
using System.Reactive.Linq;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

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

        public static IObservable<string> OnKeyDown(this UITextField text)
        {
            return Observable.FromEventPattern<EventHandler, EventArgs>(
                ev => text.EditingChanged += ev,
                ev => text.EditingChanged -= ev)
                    .Select(e=>text.Text);        
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
    }
}

