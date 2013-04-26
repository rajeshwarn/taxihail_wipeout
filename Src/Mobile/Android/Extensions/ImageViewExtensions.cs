using System;
using System.Drawing;
using Android.Widget;
using Android.Graphics.Drawables;

namespace apcurium.MK.Booking.Mobile.Client
{
	public static class ImageViewExtensions
	{
		public static Size GetBitmapSize(this ImageView thisImageView)
		{
			var drawable = (BitmapDrawable)thisImageView.Drawable;

			return new Size(drawable.Bitmap.Width,drawable.Bitmap.Height);
		}
		public static Size GetBackgroundBitmapSize(this ImageView thisImageView)
		{
			var drawable = (BitmapDrawable)thisImageView.Background;
			
			return new Size(drawable.Bitmap.Width,drawable.Bitmap.Height);
		}

	}
}

