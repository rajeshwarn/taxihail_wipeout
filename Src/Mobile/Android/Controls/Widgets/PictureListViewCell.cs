using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;


namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class PictureListViewCell : TextView
    {
        public PictureListViewCell(Context context)
            : base(context)
        {
        }

        public PictureListViewCell(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public PictureListViewCell(IntPtr ptr, JniHandleOwnership handle)
            : base(ptr, handle)
        {
        }


// ReSharper disable UnusedAutoPropertyAccessor.Global
        public string TextLeft { get; set; }

        public string TextRight { get; set; }
        public string Picture { get; set; }

        public bool IsTop { get; set; }
        public bool IsBottom { get; set; }

        public bool ShowAddSign { get; set;}

// ReSharper restore UnusedAutoPropertyAccessor.Global
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            var leftTextX = 70.ToPixels();
            var leftTextY = 30.ToPixels();
            var textSize = 17.ToPixels();
            var leftTextWidth = 75.ToPixels();

            if (!string.IsNullOrEmpty(TextLeft))
            {
                DrawText(canvas, TextLeft ?? "", leftTextX, leftTextY, textSize, Typeface.Default);
                DrawText(canvas, TextRight ?? "", leftTextX + leftTextWidth, leftTextY, textSize, Typeface.DefaultBold);
            }
            else
            {
                DrawText(canvas, TextRight ?? "", leftTextX, leftTextY, textSize, Typeface.Default);
            }

            if (ShowAddSign)
            {
                var bitmapIcon = BitmapFactory.DecodeResource (Resources, Resource.Drawable.add_list);                              
                var offsetIcon = (canvas.Height - bitmapIcon.Height) / 2;
                canvas.DrawBitmap(bitmapIcon, 10.ToPixels(), offsetIcon, null);
            }

            if (!string.IsNullOrEmpty(Picture))
            {
                var resource = Resources.GetIdentifier(Picture.ToLower(), "drawable", Context.PackageName);
                if (resource != 0)
                {
                    canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, resource), 0.ToPixels(), 3.ToPixels(),
                        null);
                }
            }

            var d = IsTop && !IsBottom
                ? Resource.Drawable.cell_top_state
                : IsBottom && !IsTop
                    ? Resource.Drawable.cell_bottom_state
                    : IsTop && IsBottom
                        ? Resource.Drawable.cell_full_state
                        : Resource.Drawable.cell_middle_state;

            SetBackgroundDrawable(Resources.GetDrawable(d));
        }

        private void DrawText(Canvas canvas, string text, float x, float y, float textSize,
            Typeface typeface)
        {
            var paintText = new TextPaint(PaintFlags.AntiAlias | PaintFlags.LinearText);
            var rect = new Rect();
            paintText.TextSize = textSize;
            paintText.GetTextBounds(text, 0, text.Length, rect);
            paintText.SetARGB(255, 49, 49, 49);
            paintText.SetTypeface(typeface);

            var p = new TextPaint();
            p.SetTypeface(typeface);

            p.TextSize = textSize;
            var ellipsizedText = TextUtils.Ellipsize(text, p, (Width - 45).ToPixels(), TextUtils.TruncateAt.End);
            if (string.IsNullOrEmpty(ellipsizedText))
            {
                ellipsizedText = text;
            }


            canvas.DrawText(ellipsizedText, x, y, paintText);
        }
    }
}