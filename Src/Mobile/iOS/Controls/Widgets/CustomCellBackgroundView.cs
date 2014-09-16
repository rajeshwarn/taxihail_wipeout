using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Style;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class CustomCellBackgroundView : UIView
	{
		private UIColor _strokeColor;
		private UIColor _selectedBackgroundColor;
		private UIColor _backgroundColor;

        public Line BottomLine;

		public CustomCellBackgroundView(RectangleF rect, float padding, UIColor backgroundColor, UIColor SelectedBackgroundColor) : base(rect)
		{
			_backgroundColor = backgroundColor;
			_selectedBackgroundColor = SelectedBackgroundColor;
            _strokeColor = Theme.IsLightContent 
                ? UIColor.FromRGB(69, 69, 69)
                : UIColor.FromRGB(190, 190, 190);

			BackgroundColor = _backgroundColor;

            BottomLine = Line.CreateHorizontal(padding, rect.Height, rect.Width - padding, _strokeColor, 1f);
            BottomLine.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            AddSubview(BottomLine);
		}

		public bool Highlighted
		{
			set
			{
				BackgroundColor = value ? _selectedBackgroundColor : _backgroundColor;
				SetNeedsDisplay();
			}
		}

		public bool HideBottomBar 
		{
			set
			{
                BottomLine.Hidden = value;
                SetNeedsDisplay();
			}
		}
	}
}

