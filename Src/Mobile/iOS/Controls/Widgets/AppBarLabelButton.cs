using System;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class AppBarLabelButton : UIButton
    {
        UILabel _label;

        public AppBarLabelButton (string text, UIColor textColor = null, UIView normal = null, UIView selected = null):base()
        {
            _label = new UILabel ();
            _label.Text = text;
            _label.Font = UIFont.FromName(FontName.HelveticaNeueLight, 34/2);
            if(textColor != null)
            {
                _label.TextColor = textColor;
            }
            else
            {
                _label.TextColor = UIColor.Black;
            }

            _label.BackgroundColor = UIColor.Clear;
            _label.TextAlignment = UITextAlignment.Center;

            SetBackgroundImage(UIImage.FromFile("highlight.png"), UIControlState.Highlighted);

            AddSubviews (_label);
        }

        public void UpdateView(float left, float top, Padding padding)
        {
            _label.SizeToFit();
            var width = _label.Frame.Width + padding.Left + padding.Right;
            var height = _label.Frame.Height + padding.Top + padding.Bottom;
            UpdateView(left, top, width, height);
        }

        public void UpdateViewRightAligned(float right, float top, Padding padding)
        {
            _label.SizeToFit();
            var width = _label.Frame.Width + padding.Left + padding.Right;
            var height = _label.Frame.Height + padding.Top + padding.Bottom;
            UpdateView(right - width, top, width, height);
        }

        public void UpdateView(float left, float top, float width, float height)
        {
            Frame = new RectangleF(left, top, width, height);

            _label.SizeToFit();
            var texttop =(height - _label.Frame.Height)/2;
            _label.SetY(texttop).SetWidth(width);
        }

        public AppBarLabelButton SetFont(UIFont font)
        {
            _label.Font = font;
            return this;
        }

        public string Text {
            get {
                return _label.Text;
            }
            set{
                _label.Text = value;
            }
        }

        public override void TouchesBegan(MonoTouch.Foundation.NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            _label.TextColor = _label.TextColor.ColorWithAlpha(0.5f);
        }

        public override void TouchesCancelled(MonoTouch.Foundation.NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            _label.TextColor = _label.TextColor.ColorWithAlpha(1f);
        }

        public override void TouchesEnded(MonoTouch.Foundation.NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            _label.TextColor = _label.TextColor.ColorWithAlpha(1f);
        }
    }
}

