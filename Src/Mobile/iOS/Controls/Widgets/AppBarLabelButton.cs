using System;
using System.Drawing;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class AppBarLabelButton : CommandButton
    {
        public AppBarLabelButton (string text, UIColor textColor = null):base()
        {
            Font = UIFont.FromName(FontName.HelveticaNeueLight, 34 / 2);
            SetTitle(text, UIControlState.Normal);
            SetTitleColor(textColor ?? UIColor.Black, UIControlState.Normal);
            SetTitleColor(TitleColor(UIControlState.Normal).ColorWithAlpha(0.5f), UIControlState.Highlighted);
            SetBackgroundImage(UIImage.FromFile("highlight.png"), UIControlState.Highlighted);
        }

        public override void Draw(RectangleF rect)
        {
            var insets = new UIEdgeInsets(0, 9, 0, 9);
            base.Draw(insets.InsetRect(rect));
        }

        public override SizeF IntrinsicContentSize
        {
            get
            {
                var insets = new UIEdgeInsets(0, 9, 0, 9);
                return SizeF.Add(base.IntrinsicContentSize, new SizeF(insets.Left + insets.Right, insets.Top + insets.Bottom));
            }
        }

    }
}

