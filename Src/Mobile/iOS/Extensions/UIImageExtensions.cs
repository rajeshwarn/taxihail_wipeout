using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
	public static class UIImageExtensions
	{
		public static UIColor GetPixel(this UIImage image, int x, int y)
		{
			var cgImage = image.CGImage.Clone ();
			var size = new Size (cgImage.Width, cgImage.Height);
			var colorSpace = CGColorSpace.CreateDeviceRGB ();
			var rawData = new byte[size.Height * size.Width * 4];
			var bytesPerPixel = 4;
			var bytesPerRow = bytesPerPixel * size.Width;
			var bitsPerComponent = 8;
			var context = new CGBitmapContext(rawData, size.Width, size.Height, bitsPerComponent, bytesPerRow, colorSpace, CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big);
			colorSpace.Dispose ();

			context.DrawImage (new RectangleF (0, 0, size.Width, size.Height), cgImage);
			context.Dispose ();

			var byteIndex = (bytesPerRow * y) + x * bytesPerPixel;
			var red = rawData[byteIndex];
			var green = rawData[byteIndex + 1];
			var blue = rawData[byteIndex + 2];
			var alpha = rawData[byteIndex + 3];

			cgImage.Dispose ();

			return UIColor.FromRGBA (red, green, blue, alpha);
		}
	}
}

