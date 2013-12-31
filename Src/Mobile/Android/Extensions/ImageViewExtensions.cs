using System.Drawing;
using Android.Graphics.Drawables;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
    public static class ImageViewExtensions
    {
        public static Size GetBitmapSize(this ImageView thisImageView)
        {
            var drawable = (BitmapDrawable) thisImageView.Drawable;

            return new Size(drawable.Bitmap.Width, drawable.Bitmap.Height);
        }

        public static Size GetBackgroundBitmapSize(this ImageView thisImageView)
        {
            var drawable = (BitmapDrawable) thisImageView.Background;

            return new Size(drawable.Bitmap.Width, drawable.Bitmap.Height);
        }
    }
}