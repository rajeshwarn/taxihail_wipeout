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
        public const float Radius = 18.0f;

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

        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            var clipPath = new Path();
            RectF rect = new RectF(0, 0, this.Width, this.Height);
            clipPath.AddRoundRect(rect, Radius, Radius, Path.Direction.Cw);
            canvas.ClipPath(clipPath);

            base.OnDraw(canvas);
        }
    }
}

