using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class CustomCellBackgroundView : UIView
	{
		private UIColor _strokeColor = UIColor.FromRGB(190, 190, 190);
		private UIColor _selectedBackgroundColor;
		private UIColor _backgroundColor;

        public Line BottomLine;

		public CustomCellBackgroundView(RectangleF rect, float padding, UIColor backgroundColor, UIColor SelectedBackgroundColor) : base(rect)
		{
			_backgroundColor = backgroundColor;
			_selectedBackgroundColor = SelectedBackgroundColor;

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

