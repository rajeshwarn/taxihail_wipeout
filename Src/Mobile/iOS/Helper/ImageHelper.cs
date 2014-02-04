using System;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Style;
using MonoTouch.CoreImage;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
	public static class ImageHelper
	{
        public static UIImage CreateFromColor(UIColor color)
        {
            var rect = new RectangleF(0f, 0f, 1f, 1f);
            UIGraphics.BeginImageContext(rect.Size);

            var context = UIGraphics.GetCurrentContext();
            context.SetFillColorWithColor(color.CGColor);
            context.FillRect(rect);

            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return image;
        }

        public static UIImage ResizeCanvas(this UIImage image, SizeF newSize)
        {
            UIGraphics.BeginImageContextWithOptions(newSize, false, 0f);

            var context = UIGraphics.GetCurrentContext();
            UIGraphics.PushContext(context);
            image.Draw(new PointF((newSize.Width - image.Size.Width) / 2, (newSize.Height - image.Size.Height) / 2));
            UIGraphics.PopContext();

            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            image = null;

            return resultImage;
        }

        public static UIImage ApplyColorToImage(string imagePath, UIColor color)
        {
            var image = UIImage.FromFile(imagePath);

            var rect = new RectangleF(0f, 0f, image.Size.Width, image.Size.Height);
            UIGraphics.BeginImageContextWithOptions(rect.Size, false, 0f);
            var context = UIGraphics.GetCurrentContext();
            image.Draw(rect);

            // translate/flip the graphics context (for transforming from CG* coords to UI* coords)
            context.TranslateCTM(0, image.Size.Height);
            context.ScaleCTM(1, -1);

            // apply clip to mask
            context.ClipToMask(rect, image.CGImage);

            // translate/flip the graphics context (for transforming from UI* coords back to CG* coords)
            context.TranslateCTM(0, image.Size.Height);
            context.ScaleCTM(1, -1);

            context.SetFillColorWithColor(color.CGColor);
            context.SetBlendMode(CGBlendMode.Hue);

            context.FillRect(rect);

            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            image = null;

            return resultImage;
        }

        public static UIImage ApplyThemeColorToImage(string imagePath)
        {
            return ApplyColorToImage(imagePath, Theme.BackgroundColor);
        }

        public static UIImage ApplyThemeTextColorToImage(string imagePath)
        {
            return ApplyColorToImage(imagePath, Theme.LabelTextColor);
        }

		public static UIImage GetImage ( string imagePath )
		{
			if (!imagePath.HasValue())
			{
                Logger.LogMessage (string.Format("Value is null for path {0}", imagePath));
				return null;
			}

            return UIImage.FromFile (imagePath);
		}
	}
}

