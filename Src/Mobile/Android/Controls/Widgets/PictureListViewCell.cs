using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class PictureListViewCell : TextView
    {
        public string TextLeft { get; set; }
        public string TextRight { get; set; }
        public string Picture { get; set; }
        public bool ShowAddSign { get; set; }
        public bool IsBottom { get; set; }

        private SizeF StandardImageSize = new SizeF(46, 32);

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

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            var leftTextX = 70.ToPixels();
            var leftTextY = 30.ToPixels();
            var textSize = 17.ToPixels();
            var leftTextWidth = 75.ToPixels();

            if (!string.IsNullOrEmpty(TextLeft))
            {
                DrawText(canvas, TextLeft ?? "", leftTextX, leftTextY, textSize, ShowAddSign ? Typeface.DefaultBold : Typeface.Default);
                DrawText(canvas, TextRight ?? "", leftTextX + leftTextWidth, leftTextY, textSize, Typeface.DefaultBold);
            }
            else
            {
                DrawText(canvas, TextRight ?? "", leftTextX, leftTextY, textSize, Typeface.Default);
            }

            if (ShowAddSign)
            {
                var bitmapIcon = BitmapFactory.DecodeResource (Resources, Resource.Drawable.add_list);                              
                var offsetTopIcon = (StandardImageSize.Height - bitmapIcon.Height) / 2;
                var offsetLeftIcon = (StandardImageSize.Width - bitmapIcon.Width) / 2;
                canvas.DrawBitmap(bitmapIcon, offsetLeftIcon + 10, offsetTopIcon + 10, null);
            }

            if (!string.IsNullOrEmpty(Picture))
            {
                var resource = Resources.GetIdentifier(Picture.ToLower(), "drawable", Context.PackageName);
                if (resource != 0)
                {
                    var bitmapIcon = BitmapFactory.DecodeResource(Resources, resource);
                    var offsetTopIcon = (StandardImageSize.Height - bitmapIcon.Height) / 2;
                    var offsetLeftIcon = (StandardImageSize.Width - bitmapIcon.Width) / 2;
                    canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, resource), offsetLeftIcon + 10, offsetTopIcon + 10, null);
                }
            }

            SetBackgroundDrawable(Resources.GetDrawable(IsBottom 
                ? Resource.Drawable.cell_bottom_state 
                : Resource.Drawable.cell_middle_state));
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