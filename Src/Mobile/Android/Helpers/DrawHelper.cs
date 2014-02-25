using Android.App;
using Android.Graphics;
using Android.Util;
using apcurium.MK.Booking.Mobile.Style;
using System;
using Android.Gms.Maps.Model;
using Android.Graphics.Drawables;
using Android.Views;

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
            var px =
                (int)
                    TypedValue.ApplyDimension(ComplexUnitType.Dip, dipValue,
                        Application.Context.Resources.DisplayMetrics); // getDisplayMetrics());
            return px;
        }

        public static int GetPixelsFromPt(float ptValue)
        {
            var px =
                (int)
                    TypedValue.ApplyDimension(ComplexUnitType.Pt, ptValue, Application.Context.Resources.DisplayMetrics);
                // getDisplayMetrics());
            return px;
        }

        public static Color ConvertToColor(this ColorDefinition colorDef)
        {
            return new Color(colorDef.Red, colorDef.Green, colorDef.Blue, colorDef.Alpha);
        }


        public static Bitmap DrawableToBitmap (Drawable drawable, Color? colorFilter = null) 
        {
            Bitmap bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(bitmap); 

            drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
            drawable.Draw(canvas);

            if (colorFilter != null)
            {
                bitmap = Colorize(bitmap, (Color)colorFilter);
            }

            return bitmap;
        }

        public static BitmapDescriptor DrawableToBitmapDescriptor (Drawable drawable, Color? colorFilter = null) 
        {
            // Refactorize to use Drawable to bitmap
            Bitmap bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(bitmap); 

            drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
            drawable.Draw(canvas);

            if (colorFilter != null)
            {
                bitmap = Colorize(bitmap, (Color)colorFilter);
            }

            return BitmapDescriptorFactory.FromBitmap(bitmap);
        }

        public static Bitmap Colorize(Bitmap src, Color colorFilter) {
            // TODO: No real usage of HSV, but keep method intact for future cases 
            Bitmap b = src.Copy(src.GetConfig(), true);
            for (int x = 0; x < b.Width; x++) {
                for (int y = 0; y < b.Height; y++) {
                    int color = b.GetPixel(x, y);
                    float[] hsv = new float[3];
                    hsv = ColorToHSV(new Color(color));
                    float[] hsvMask = ColorToHSV(new Color(colorFilter));
                    Color newColor = HSVToColor(hsvMask[0], hsv[1], hsvMask[2]);
                    newColor.A = new Color(color).A;
                    if (Color.GetAlphaComponent(color) != 0)
                    {
                        b.SetPixel(x, y, newColor);
                    }
                }
            }

            return b;
        }

        public static Bitmap Blur(Bitmap src) 
        {
            if (src != null)
            {
                int w = src.Width;
                int h = src.Height;
                RectF rectF = new RectF(-100, -100, w + 100, h + 100);
                float blurRadius = w;
                Bitmap bitmapResult = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);
                Canvas canvasResult = new Canvas(bitmapResult);
                Paint blurPaintInner = new Paint();
                blurPaintInner.Color = new Color(0, 0, 0, 100);
                blurPaintInner.SetMaskFilter(new BlurMaskFilter(blurRadius, BlurMaskFilter.Blur.Normal));
                canvasResult.DrawRect(rectF, blurPaintInner);
                return bitmapResult;
            }
            return src; 
        }

        public static Bitmap LoadBitmapFromView(View v, System.Drawing.Size size) {
            Bitmap b = Bitmap.CreateBitmap(size.Width , size.Height, Bitmap.Config.Argb8888);
            Canvas c = new Canvas(b);
            v.Layout(0, 0, size.Width, size.Height);
            v.Draw(c);
            return b;
        }

        public static float[] ColorToHSV(Color color)
        {
            float max = Math.Max(color.R, Math.Max(color.G, color.B));
            float min = Math.Min(color.R, Math.Min(color.G, color.B));
            float[] result = new float[3];
            result[0] = color.GetHue() > 359 ? 0 : color.GetHue(); // hue
            result[1] = (max == 0) ? 0 : 1f - (1f * min / max); // saturation
            result[2] = max / 255f; // value
            return result;
        }

        public static Color HSVToColor(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = hue / 60 - (float)Math.Floor(hue / 60);

            value = Math.Min(value, 1) * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.Argb(255, v, t, p);
            else if (hi == 1)
                return Color.Argb(255, q, v, p);
            else if (hi == 2)
                return Color.Argb(255, p, v, t);
            else if (hi == 3)
                return Color.Argb(255, p, q, v);
            else if (hi == 4)
                return Color.Argb(255, t, p, v);
            else
                return Color.Argb(255, v, p, q);
        }
    }
}