using CoreGraphics;
using CoreImage;
using UIKit;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Style;
using System;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
	public static class ImageHelper
	{
        public static UIImage CreateFromColor(UIColor color)
        {
            var rect = new CGRect(0f, 0f, 1f, 1f);
            UIGraphics.BeginImageContext(rect.Size);

            var context = UIGraphics.GetCurrentContext();
            context.SetFillColor(color.CGColor);
            context.FillRect(rect);

            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return image;
        }

        public static UIImage ResizeCanvas(this UIImage image, CGSize newSize)
        {
            UIGraphics.BeginImageContextWithOptions(newSize, false, 0f);

            var context = UIGraphics.GetCurrentContext();
            UIGraphics.PushContext(context);
            image.Draw(new CGPoint((newSize.Width - image.Size.Width) / 2, (newSize.Height - image.Size.Height) / 2));
            UIGraphics.PopContext();

            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            image = null;

            return resultImage;
        }

        public static UIImage ApplyColorToMapIcon(string imagePath, UIColor color, bool bigIcon = true)
        {
            var image = GetImage(imagePath);

            var originalImageSize = bigIcon 
                ? new CGSize(52, 58)
                : new CGSize(34, 39);

            if (ImageWasOverridden(image, originalImageSize, UIColor.FromRGBA(0, 0, 0, 0), bigIcon ? new CGPoint(26, 29) : new CGPoint(18, 16)))
            {
                return image;
            }

            var backgroundToColorize = bigIcon
                ? UIImage.FromBundle ("map_bigicon_background")
                : UIImage.FromBundle ("map_smallicon_background");

            var rect = new CGRect(0f, 0f, backgroundToColorize.Size.Width, backgroundToColorize.Size.Height);
            UIGraphics.BeginImageContextWithOptions(rect.Size, false, 0f);
            var context = UIGraphics.GetCurrentContext();

            // Step 1: Add the background (this is only a grayscale bubble)
            backgroundToColorize.Draw(rect);

            // translate/flip the graphics context (for transforming from CG* coords to UI* coords)
            context.TranslateCTM(0, backgroundToColorize.Size.Height);
            context.ScaleCTM(1, -1);

            // Step 2: Apply clip to mask to prevent the colorize from creating a colorized rectangle
            context.ClipToMask (rect, backgroundToColorize.CGImage);

            // Step 3: Colorize the background
            context.SetFillColor (color.CGColor);
            context.SetBlendMode (CGBlendMode.SourceIn);

            // Step 4: Add the white portion on top of background
            var imageForeground = UIImage.FromFile (imagePath);
            context.DrawImage (rect, imageForeground.CGImage);
            context.SetBlendMode (CGBlendMode.DestinationOver);

            context.FillRect(rect);

            // translate/flip the graphics context (for transforming from UI* coords back to CG* coords)
            context.TranslateCTM(0, backgroundToColorize.Size.Height);
            context.ScaleCTM(1, -1);

            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            backgroundToColorize = null;
            imageForeground = null;

            return resultImage;
        }

        public static UIImage ApplyColorToImage(string imagePath, UIColor color)
        {
            var image = UIImage.FromFile(imagePath);

            var rect = new CGRect(0f, 0f, image.Size.Width, image.Size.Height);
            UIGraphics.BeginImageContextWithOptions(rect.Size, false, 0f);
            var context = UIGraphics.GetCurrentContext();
            image.Draw(rect);

            // translate/flip the graphics context (for transforming from CG* coords to UI* coords)
            context.TranslateCTM(0, image.Size.Height);
            context.ScaleCTM(1, -1);

            // apply clip to mask
            context.ClipToMask (rect, image.CGImage);

            context.SetFillColor(color.CGColor);
            context.SetBlendMode(CGBlendMode.Normal);

            context.FillRect(rect);

            // translate/flip the graphics context (for transforming from UI* coords back to CG* coords)
            context.TranslateCTM(0, image.Size.Height);
            context.ScaleCTM(1, -1);

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

            var croppedImage = resultImage.ImageByCroppingToRect(new CGRect(blurRadius, blurRadius, _size.Width - 2*blurRadius, _size.Height - 2*blurRadius));              
            var transformFilter = new CIAffineTransform();
            var affineTransform = CGAffineTransform.MakeTranslation (-blurRadius, blurRadius);
            transformFilter.Transform = affineTransform;
            transformFilter.Image = croppedImage;
            var transformedImage = transformFilter.OutputImage;

            return new UIImage(transformedImage);
        }

        public static UIImage ApplyThemeColorToMapIcon(string imagePath, bool isBigIcon)
        {
            return ApplyColorToMapIcon(imagePath, Theme.CompanyColor, isBigIcon);
        }

        public static UIImage ApplyThemeColorToImage(string imagePath, bool skipApplyIfCustomImage = false, CGSize originalImageSize = new CGSize(), UIColor expectedColor = null, CGPoint? expectedColorCoordinate = null)
        {
            if (skipApplyIfCustomImage)
            {
                var image = GetImage(imagePath);

                if (ImageWasOverridden(image, originalImageSize, expectedColor, expectedColorCoordinate))
                {
                    return image;
                }
            }
            return ApplyColorToImage(imagePath, Theme.CompanyColor);
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

        private static bool ImageWasOverridden(UIImage image, CGSize originalImageSize, UIColor expectedColor, CGPoint? expectedColorCoordinate)
        {
            var differentSize = image.Size.Width != originalImageSize.Width;
            if (differentSize)
            {
                return true;
            }

            if (expectedColor == null || expectedColorCoordinate == null)
            {
                return false;
            }
                
            var detectedColor = GetPixel(image, (int)expectedColorCoordinate.Value.X, (int)expectedColorCoordinate.Value.Y);
            var differentColorThanExpected = !detectedColor.CGColor.Equals(expectedColor.CGColor);

            return differentColorThanExpected;
        }

        private static UIColor GetPixel(UIImage image, int x, int y)
        {
            var correctedX = (int)(image.CurrentScale * x);
            var correctedY = (int)(image.CurrentScale * y);

            var cgImage = image.CGImage.Clone ();
            var colorSpace = CGColorSpace.CreateDeviceRGB ();
            var rawData = new byte[(int)(cgImage.Height * cgImage.Width * 4)];
            var bytesPerPixel = 4;
            var bytesPerRow = bytesPerPixel * cgImage.Width;
            var bitsPerComponent = 8;
            var context = new CGBitmapContext(rawData, cgImage.Width, cgImage.Height, bitsPerComponent, bytesPerRow, colorSpace, CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big);
            colorSpace.Dispose ();

            context.DrawImage (new CGRect (0, 0, cgImage.Width, cgImage.Height), cgImage);
            context.Dispose ();

            var byteIndex = (bytesPerRow * correctedY) + correctedX * bytesPerPixel;
            var red = rawData[(int)byteIndex];
            var green = rawData[(int)byteIndex + 1];
            var blue = rawData[(int)byteIndex + 2];
            var alpha = rawData[(int)byteIndex + 3];

            cgImage.Dispose ();

            return UIColor.FromRGBA (red, green, blue, alpha);
        }

        public static UIImage ImageToOrientedMapIcon(string imagePath, double degrees, bool bigIcon = true)
        {
            var image = GetImage(imagePath);

            // Step 1: Calculate the size of the rotated view's containing box for our drawing space
            UIView rotatedViewBox = new UIView(new CGRect(0,0,image.Size.Width, image.Size.Height));

            rotatedViewBox.Transform = CGAffineTransform.MakeRotation((nfloat)(degrees * Math.PI / 180));
            CGSize rotatedSize = rotatedViewBox.Frame.Size;
            rotatedViewBox = null;

            // Step 2: Create the bitmap context
            UIGraphics.BeginImageContext(rotatedSize);
            CGContext bitmap = UIGraphics.GetCurrentContext();

            // Step 3: Move the origin to the middle of the image so we will rotate and scale around the center.
            bitmap.TranslateCTM(rotatedSize.Width/2, rotatedSize.Height/2);

            // Step 4: Rotate the image context
            bitmap.RotateCTM((nfloat)(degrees * Math.PI / 180));

            // Step 5: Now, draw the rotated/scaled image into the context
            bitmap.ScaleCTM((nfloat)1.0, (nfloat)(-1.0));
            bitmap.DrawImage(new CGRect(-image.Size.Width / 2, -image.Size.Height / 2, image.Size.Width, image.Size.Height), image.CGImage);

            UIImage resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return resultImage;
        }
	}
}

