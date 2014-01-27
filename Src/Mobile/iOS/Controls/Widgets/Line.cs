using System;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class Line : UIView 
	{
        public static Line CreateHorizontal(float width, UIColor color)
        {
            return CreateHorizontal(width, color, 1f);
        }

        public static Line CreateHorizontal(float width, UIColor color, float thickness)
        {
            return CreateHorizontal(0, 0, width, color, thickness);
        }

        public static Line CreateHorizontal(float x, float y, float width, UIColor color, float thickness)
        {
            return new Line(new RectangleF(x, y, width, UIHelper.GetConvertedPixel(thickness)))
            {
                BackgroundColor = color
            };
        }

        public static Line CreateVertical(float height, UIColor color)
        {
            return CreateVertical(0, 0, height, color, 1f);
        }

        public static Line CreateVertical(float x, float height, UIColor color)
        {
            return CreateVertical(x, 0, height, color, 1f);
        }

        public static Line CreateVertical(float x, float y, float height, UIColor color, float thickness)
        {
            return new Line(new RectangleF(x, y, UIHelper.GetConvertedPixel(thickness), height))
            {
                BackgroundColor = color
            };
        }

        public Line(RectangleF rect) : base(rect)
		{
		}
	}
}

