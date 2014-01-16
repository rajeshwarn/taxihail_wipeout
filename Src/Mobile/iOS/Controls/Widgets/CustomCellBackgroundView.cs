using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class CustomCellBackgroundView : UIView
	{
		private UIColor _strokeColor = UIColor.FromRGB(190, 190, 190);
		private UIColor _selectedBackgroundColor = UIColor.FromRGB(190, 190, 190);
		private UIColor _backgroundColor = UIColor.White;

		private Line _bottomLine;

		public CustomCellBackgroundView(RectangleF rect) : base(rect)
		{			
			BackgroundColor = _backgroundColor;

			_bottomLine = new Line(10, rect.Height - 1, rect.Width - 10, 1, _strokeColor);
			_bottomLine.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			AddSubview(_bottomLine);
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
				_bottomLine.Hidden = value;
				SetNeedsDisplay();
			}
		}
	}
}

