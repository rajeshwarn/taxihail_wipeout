using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class BaloonLayout : LinearLayout
    {
        public BaloonLayout(Context context) : base(context)
        {
        }

        public BaloonLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public BaloonLayout(IntPtr ptr, JniHandleOwnership handle)
            : base(ptr, handle)
        {
        }

        protected override void DispatchDraw(Canvas canvas)
        {
            var panelPaint = new Paint();
            panelPaint.SetARGB(0, 0, 0, 0);

            var panelRect = new RectF();

            panelRect.Set(0, 0, MeasuredWidth, MeasuredHeight);
            canvas.DrawRoundRect(panelRect, 5, 5, panelPaint);

            var baloonRect = new RectF();
            baloonRect.Set(0, 0, MeasuredWidth, 2*(MeasuredHeight/3));
            panelPaint.SetARGB(230, 255, 255, 255);
            canvas.DrawRoundRect(baloonRect, 10, 10, panelPaint);

            var baloonTip = new Path();
            baloonTip.MoveTo(5*(MeasuredWidth/8), 2*(MeasuredHeight/3));
// ReSharper disable once PossibleLossOfFraction
            baloonTip.LineTo(MeasuredWidth/2, MeasuredHeight);
            baloonTip.LineTo(3*(MeasuredWidth/4), 2*(MeasuredHeight/3));

            canvas.DrawPath(baloonTip, panelPaint);

            base.DispatchDraw(canvas);
        }
    }
}