using System;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Graphics;
using Android.Content;
using Android.Runtime;
using Android.Util;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register("apcurium.mk.booking.mobile.client.controls.RoundedMvxImageView")]
    public class RoundedMvxImageView : MvxImageView
    {
        [Register(".ctor", "(Landroid/content/Context;)V", "")]
        public RoundedMvxImageView(Context context)
            : base (context)
        {
        }

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
        public RoundedMvxImageView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public override void SetImageBitmap(Bitmap bm)
        {
            if (bm != null)
            {
				var width = Width > 0
					? Width
					: bm.Width;
				var height = Height > 0
					? Height
					: bm.Height;
				
                var output = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
                var canvas = new Canvas(output);

                var paint = new Paint();
                paint.AntiAlias = true;
                paint.Color = Color.Black;

				canvas.DrawARGB(0, 0, 0, 0);
				canvas.DrawCircle(width / 2, height / 2, width / 2, paint);

				paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));

				var rect = new Rect(0, 0, width, height);
				canvas.DrawBitmap(bm, null, rect, paint);

                base.SetImageBitmap(output);
                return;
            }

            base.SetImageBitmap(bm);
        }
    }
}

