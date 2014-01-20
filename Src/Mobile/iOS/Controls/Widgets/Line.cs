using System;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class Line : UIView 
	{
		private float _x;
		private float _y;
		private float _thickness;
		private float _width;

		public Line(float x, float y, float width, float thickness, UIColor color)
		{
			_x =x ;
			_y = y;
			_width = width;
			_thickness =thickness;
			BackgroundColor = color;
			Frame = Frame;
		}

		public override System.Drawing.RectangleF Frame
		{
			get
			{
				float thickness = UIHelper.IsRetinaDisplay ? _thickness / 2 : _thickness ;
				return new System.Drawing.RectangleF( _x, _y - thickness, _width , thickness );
			}
			set
			{
				base.Frame = value;
			}
		}
	}
}

