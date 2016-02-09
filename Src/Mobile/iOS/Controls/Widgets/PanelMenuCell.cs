using Cirrious.MvvmCross.Binding.Touch.Views;
using UIKit;
using Foundation;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Style;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class PanelMenuCell : MvxStandardTableViewCell
    {
        private CustomBadgeView _badgeView;
        private string _badgeAlertText;
		private bool _hideBottomBar;
        private const float LeftPadding = 16f;
        private const float RightPadding = 18f;

		public PanelMenuCell (string cellIdentifier, string bindingText)
			: base (bindingText, UITableViewCellStyle.Default, new NSString (cellIdentifier), UITableViewCellAccessory.None)
		{
            BackgroundColor = UIColor.Clear;

            // this color is added on top of the menu color
            // 0.04 value for alpha was obtained by a picky client wanting a specific selected color
            var selectedColorDelta = Theme.ShouldHaveLightContent(Theme.MenuColor)
                ? UIColor.White.ColorWithAlpha (0.04f)
                : UIColor.Black.ColorWithAlpha (0.04f);
            BackgroundView = new CustomCellBackgroundView(Frame, LeftPadding, UIColor.Clear, selectedColorDelta);

            SelectionStyle = UITableViewCellSelectionStyle.None;
			Accessory = UITableViewCellAccessory.None;

			TextLabel.Font = UIFont.FromName (FontName.HelveticaNeueLight, 36 / 2);
            TextLabel.TextColor = Theme.ShouldHaveLightContent(Theme.MenuColor)
                ? UIColor.White
                : UIColor.FromRGB (79, 76, 71);
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

            ((CustomCellBackgroundView)BackgroundView).BottomLine.SetWidth (Frame.Width - LeftPadding - RightPadding);

            TextLabel.SetX(20);
            TextLabel.TextAlignment = UITextAlignment.Natural;
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
            

        public string AlertBadgeText
        {
            get { return _badgeAlertText; }
            set
            {
                if (_badgeAlertText != value)
                {
					if (_badgeView != null)
					{
						_badgeView.RemoveFromSuperview ();
						_badgeView = null;
					}

                    if (value.HasValue())
                    {
                        _badgeView = new CustomBadgeView(value, 120, 10)
						{
                            TextColor = Theme.LabelTextColor,
                            BadgeColor = Theme.CompanyColor
                        };

                        AddSubview(_badgeView);
                    }

                    _badgeAlertText = value;
                }
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

