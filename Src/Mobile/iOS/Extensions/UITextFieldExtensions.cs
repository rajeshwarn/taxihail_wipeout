using System;
using UIKit;
using CoreGraphics;
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
    }
}

