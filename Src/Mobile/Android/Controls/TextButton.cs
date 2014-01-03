using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class TextButton : Button
    {
        public TextButton(Context context)
            : base(context)
        {
        }

        public TextButton(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public TextButton(IntPtr ptr, JniHandleOwnership handle)
            : base(ptr, handle)
        {
        }


// ReSharper disable UnusedAutoPropertyAccessor.Global
        public string TextLine1 { get; set; }
        public string TextLine2 { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            DrawText(canvas, TextLine1 ?? "", 8, 20, 15);
            DrawText(canvas, TextLine2 ?? "", 8, 45, 20);
        }

        private void DrawText(Canvas canvas, string text, float x, float y, float textSize)
        {
            var paintText = new TextPaint(PaintFlags.AntiAlias | PaintFlags.LinearText);
            var rect = new Rect();

            paintText.TextSize = textSize; // / DisplayMetrics.DensityDefault ;
            paintText.GetTextBounds(text, 0, text.Length, rect);
            paintText.SetARGB(255, 49, 49, 49);
            paintText.SetTypeface(AppFonts.Regular);
            canvas.DrawText(text, x, y, paintText);
        }
    }
}