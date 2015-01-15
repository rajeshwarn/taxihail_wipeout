using UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using CoreGraphics;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class Line : UIView 
	{
        public static Line CreateHorizontal(nfloat width, UIColor color)
        {
            return CreateHorizontal(width, color, 1f);
        }

        public static Line CreateHorizontal(nfloat width, UIColor color, nfloat thickness)
        {
            return CreateHorizontal(0, 0, width, color, thickness);
        }

        public static Line CreateHorizontal(nfloat x, nfloat y, nfloat width, UIColor color, nfloat thickness)
        {
            var convertedThickness = UIHelper.GetConvertedPixel(thickness);

            return new Line(new CGRect(x, y, width, convertedThickness))
            {
                BackgroundColor = color,
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth
            };
        }

        public static Line CreateVertical(nfloat height, UIColor color)
        {
            return CreateVertical(0, 0, height, color, 1f);
        }

        public static Line CreateVertical(nfloat x, nfloat height, UIColor color)
        {
            return CreateVertical(x, 0, height, color, 1f);
        }

        public static Line CreateVertical(nfloat x, nfloat y, nfloat height, UIColor color, nfloat thickness)
        {
            var convertedThickness = UIHelper.GetConvertedPixel(thickness);

            return new Line(new CGRect(x - convertedThickness, y, convertedThickness, height))
            {
                BackgroundColor = color,
                AutoresizingMask = UIViewAutoresizing.FlexibleHeight
            };
        }

        private Line(CGRect rect) : base(rect)
		{
		}
	}
}

