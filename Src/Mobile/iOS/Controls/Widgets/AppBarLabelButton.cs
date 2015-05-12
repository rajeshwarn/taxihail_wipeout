using CoreGraphics;
using UIKit;

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

        public override void Draw(CGRect rect)
        {
            var insets = new UIEdgeInsets(0, 9, 0, 9);
            base.Draw(insets.InsetRect(rect));
        }

        public override CGSize IntrinsicContentSize
        {
            get
            {
                var insets = new UIEdgeInsets(0, 9, 0, 9);
                return CGSize.Add(base.IntrinsicContentSize, new CGSize(insets.Left + insets.Right, insets.Top + insets.Bottom));
            }
        }

    }
}

