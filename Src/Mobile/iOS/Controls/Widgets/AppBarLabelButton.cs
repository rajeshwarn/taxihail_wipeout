using CoreGraphics;
using UIKit;
using Foundation;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register("AppBarLabelButton")]
    public class AppBarLabelButton : CommandButton
    {
		public AppBarLabelButton(IntPtr handle):base(handle)
		{
		}

		public AppBarLabelButton(CGRect frame):base(frame)
		{
		}

		public AppBarLabelButton():base()
		{
		}


        public AppBarLabelButton (string text, UIColor textColor = null):base()
        {
			Font = UIFont.FromName(FontName.HelveticaNeueLight, 34 / 2);
			SetTitle(text, UIControlState.Normal);
			SetTitleColor(textColor ?? UIColor.Black, UIControlState.Normal);
			SetTitleColor(TitleColor(UIControlState.Normal).ColorWithAlpha(0.5f), UIControlState.Highlighted);
			SetBackgroundImage(UIImage.FromFile("highlight.png"), UIControlState.Highlighted);
        }





		public void Initialize(string text, UIColor textColor = null)
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