using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;

using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class ListViewCell2 : TextView
    {
        private bool _isBottom;
        private bool _isTop;

        public ListViewCell2(Context context)
            : base(context)
        {
            Ctor();
        }

        public ListViewCell2(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Ctor();
        }

        public ListViewCell2(IntPtr ptr, JniHandleOwnership handle)
            : base(ptr, handle)
        {
            Ctor();
        }

// ReSharper disable UnusedAutoPropertyAccessor.Global
        public string TextLine1 { get; set; }

        public string TextLine2 { get; set; }

        public bool ShowRightArrow { get; set; }
        public bool ShowPlusSign { get; set; }

        public string Icon { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global

        public Color TextColorLine1 { get; set; }

        public bool IsTop
        {
            get { return _isTop; }
            set
            {
                _isTop = value;
                Invalidate();
            }
        }

        public bool IsBottom
        {
            get { return _isBottom; }
            set
            {
                _isBottom = value;
                Invalidate();
            }
        }

       

        public void Ctor()
        {
			TextColorLine1 = Resources.GetColor (Resource.Color.listitem_text_line1_color);
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

			var TextColorLine2 = Resources.GetColor (Resource.Color.listitem_text_line2_color);

            //var textX = ShowPlusSign ? 65.ToPixels() : 8.ToPixels();
			var textX = 8.ToPixels();

            if (ShowPlusSign || Icon.HasValue())
            {
                textX = 40.ToPixels();
            }

            if (!TextLine1.IsNullOrEmpty())
            {
                DrawText(canvas, TextLine1, textX, 21.ToPixels(), 16.ToPixels(), AppFonts.Bold, TextColorLine1);
                DrawText(canvas, TextLine2 ?? "", textX, 41.ToPixels(), 15.ToPixels(), AppFonts.Regular, TextColorLine2);
            }
            else
            {
                DrawText(canvas, TextLine2 ?? "", textX, 32.ToPixels(), 16.ToPixels(), AppFonts.Regular, TextColorLine2);
            }

            if (ShowRightArrow)
            {                
				canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, Resource.Drawable.right_arrow),
                    Width - (20.ToPixels()), 16.ToPixels(), null);
            }

            if (ShowPlusSign)
            {
                canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, Resource.Drawable.add_location), 6.ToPixels(),
                    15.ToPixels(), null);
            }
            else if (Icon.HasValue())
            {
                var identifier = Context.Resources.GetIdentifier(Icon, "drawable", Context.PackageName);


                canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, identifier), 6.ToPixels(), 15.ToPixels(), null);
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

        private void DrawText(Canvas canvas, string text, float x, float y, float textSize, Typeface typeface,
            Color color)
        {
            var paintText = new TextPaint(PaintFlags.LinearText | PaintFlags.AntiAlias)
            {
                TextSize = textSize
            };

            paintText.GetTextBounds(text, 0, text.Length, new Rect());
            paintText.SetARGB(color.A, color.R, color.G, color.B);
            paintText.SetTypeface(typeface);

            var p = new TextPaint();
            p.SetTypeface(typeface);

            p.TextSize = textSize;
            var ellipsizedText = TextUtils.Ellipsize(text, p, Width - 75.ToPixels(), TextUtils.TruncateAt.End);
            if (ellipsizedText.IsNullOrEmpty())
            {
                ellipsizedText = text;
            }

            canvas.DrawText(ellipsizedText, x, y, paintText);
        }
    }
}