using System;
using Android.App;
using Android.Content.Res;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Style;
using System.Drawing;
using Color = Android.Graphics.Color;

namespace apcurium.MK.Booking.Mobile.Client.Helpers
{
    public static class DrawHelper
    {
        public static int ToPixels(this float dip)
        {
            return GetPixels(dip);
        }

        public static int ToPixels(this int dip)
        {
            return GetPixels(dip);
        }

        public static int GetPixels(float dipValue)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dipValue,Application.Context.Resources.DisplayMetrics);
        }

        public static int GetPixelsFromPt(float ptValue)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Pt, ptValue, Application.Context.Resources.DisplayMetrics);
        }

        public static Bitmap DrawableToBitmap (Drawable drawable, Color? colorFilter = null) 
        {
            if (colorFilter != null)
            {
                drawable.SetColorFilter ((Color)colorFilter, PorterDuff.Mode.SrcIn);
            }

            var bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
            var canvas = new Canvas(bitmap); 

            drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
            drawable.Draw(canvas);

            return bitmap;
        }

        public static BitmapDescriptor DrawableToBitmapDescriptor (Drawable drawable, Color? colorFilter = null) 
        {
            return BitmapDescriptorFactory.FromBitmap(DrawableToBitmap(drawable, colorFilter));
        }

        public static Bitmap GetMapIconBitmap(Drawable foreground, Color color, Drawable backgroundToColorize, SizeF originalImageSize)
        {
            var imageWasOverridden = foreground.IntrinsicWidth != originalImageSize.Width.ToPixels();
            if (imageWasOverridden)
            {
                return DrawableToBitmap(foreground);
            }

            var bitmapOverlay = Bitmap.CreateBitmap(backgroundToColorize.IntrinsicWidth, backgroundToColorize.IntrinsicHeight, Bitmap.Config.Argb8888);
            var canvas = new Canvas(bitmapOverlay);

            canvas.DrawBitmap (DrawableToBitmap (backgroundToColorize, color), new Matrix (), null);
            canvas.DrawBitmap (DrawableToBitmap (foreground), 0, 0, null);

            return bitmapOverlay;
        }

        public static BitmapDescriptor GetMapIcon(Drawable foreground, Color color, Drawable backgroundToColorize, SizeF originalImageSize)
        {
            return BitmapDescriptorFactory.FromBitmap(GetMapIconBitmap(foreground, color, backgroundToColorize, originalImageSize));
        }

        public static Color GetTextColorForBackground(Color backgroundColor)
        {
            var darknessScore = (((backgroundColor.R) * 299) + ((backgroundColor.G) * 587) + ((backgroundColor.B) * 114)) / 1000;

            if (darknessScore >= 125) 
            {
                return Color.Black;
            }

            return Color.White;
        }

        public static void SupportLoginTextColor(TextView textView)
        {
            int[][] states = new int[1][];
            states[0] = new int[0];
            var colors = new[]{(int)GetTextColorForBackground(textView.Resources.GetColor(Resource.Color.login_color))};
            var colorList = new ColorStateList (states, colors);
            textView.SetTextColor(colorList);
        }

		public static bool CheckForAssetOverride(Bitmap image, Color expectedColor, Point expectedColorCoordinate)
		{
			var detectedColor = image.GetPixel (expectedColorCoordinate.X, expectedColorCoordinate.Y);
			return !detectedColor.Equals(expectedColor.ToArgb());
		}
    }
}