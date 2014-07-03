using Cirrious.MvvmCross.Binding.Touch.Views;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class PanelMenuCell : MvxStandardTableViewCell
	{
		private bool _hideBottomBar;
        private const float LeftPadding = 16f;
        private const float RightPadding = 18f;

		public PanelMenuCell (string cellIdentifier, string bindingText)
			: base (bindingText, UITableViewCellStyle.Default, new NSString (cellIdentifier), UITableViewCellAccessory.None)
		{
            BackgroundColor = UIColor.Clear;
            BackgroundView = new CustomCellBackgroundView(Frame, LeftPadding, UIColor.Clear, UIColor.White);

            SelectionStyle = UITableViewCellSelectionStyle.None;
			Accessory = UITableViewCellAccessory.None;

			TextLabel.Font = UIFont.FromName (FontName.HelveticaNeueLight, 36 / 2);
			TextLabel.TextColor = UIColor.FromRGB (79, 76, 71);
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

            ((CustomCellBackgroundView)BackgroundView).BottomLine.SetWidth (Frame.Width - LeftPadding - RightPadding);

            TextLabel.SetX(20);

			// we can't use UITextAlignment.Natural on iOS6 apparently 
			// (NSInvalidArgumentException Reason: textAlignment does not accept NSTextAlignmentNatural)
			// we detect arabic, since it's currently the only RTL language we support
			if (this.Services ().Localize.IsRightToLeft)
			{
				TextLabel.TextAlignment = UITextAlignment.Right;
			}
		}

		public bool HideBottomBar
		{
			get { return _hideBottomBar; }
			set
			{ 
				if (BackgroundView is CustomCellBackgroundView)
				{
					((CustomCellBackgroundView)BackgroundView).HideBottomBar = value;
				}
				_hideBottomBar = value;
			}
		}

        public override void TouchesBegan (NSSet touches, UIEvent evt)
        {   
            ((CustomCellBackgroundView)BackgroundView).Highlighted = true;
            SetNeedsDisplay();
            base.TouchesBegan ( touches, evt ); 
        }

        public override void TouchesEnded (NSSet touches, UIEvent evt)
        {
            ((CustomCellBackgroundView) BackgroundView).Highlighted = false;
            SetNeedsDisplay();
            base.TouchesEnded (touches, evt);
        }

        public override void TouchesCancelled (NSSet touches, UIEvent evt)
        {
            ((CustomCellBackgroundView) BackgroundView).Highlighted = false;
            SetNeedsDisplay();
            base.TouchesCancelled (touches, evt);
        }
	}
}

