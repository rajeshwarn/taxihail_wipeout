using System;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using apcurium.MK.Booking.Mobile.Client.Style;
using MonoTouch.CoreImage;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;

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

        public static UIImage CreateBlurImageFromView(UIView view)
        {
            var blurRadius = 2f;

            var _size = view.Bounds.Size;

            UIGraphics.BeginImageContext(_size);
            if (UIHelper.IsOS7orHigher)
            {
                // use faster approach available on iOS7
                view.DrawViewHierarchy(view.Bounds, false);
            }
            else
            {
                view.Layer.RenderInContext(UIGraphics.GetCurrentContext());
            }
            var viewImage = UIGraphics.GetImageFromCurrentImageContext();

            // Blur Image
            var gaussianBlurFilter = new CIGaussianBlur();
            gaussianBlurFilter.Image = CIImage.FromCGImage(viewImage.CGImage);
            gaussianBlurFilter.Radius = blurRadius;
            var resultImage = gaussianBlurFilter.OutputImage;

            var croppedImage = resultImage.ImageByCroppingToRect(new RectangleF(blurRadius, blurRadius, _size.Width - 2*blurRadius, _size.Height - 2*blurRadius));              
            var transformFilter = new CIAffineTransform();
            var affineTransform = CGAffineTransform.MakeTranslation (-blurRadius, blurRadius);
            transformFilter.Transform = affineTransform;
            transformFilter.Image = croppedImage;
            var transformedImage = transformFilter.OutputImage;

            return new UIImage(transformedImage);
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

