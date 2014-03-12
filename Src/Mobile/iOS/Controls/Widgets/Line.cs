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
            var convertedThickness = UIHelper.GetConvertedPixel(thickness);

            return new Line(new RectangleF(x, y - convertedThickness, width, convertedThickness))
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
            var convertedThickness = UIHelper.GetConvertedPixel(thickness);

            return new Line(new RectangleF(x - convertedThickness, y, convertedThickness, height))
            {
                BackgroundColor = color
            };
        }

        private Line(RectangleF rect) : base(rect)
		{
		}
	}
}

