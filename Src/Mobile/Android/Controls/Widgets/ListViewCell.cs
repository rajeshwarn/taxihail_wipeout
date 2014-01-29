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
    public class ListViewCell : TextView
    {
        private bool _isBottom;
        private bool _isTop;

        public ListViewCell(Context context)
            : base(context)
        {
            Ctor();
        }

        public ListViewCell(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Ctor();
        }

        public ListViewCell(IntPtr ptr, JniHandleOwnership handle)
            : base(ptr, handle)
        {
            Ctor();
        }

// ReSharper disable UnusedAutoPropertyAccessor.Global
        public string TextLine1 { get; set; }

        public string TextLine2 { get; set; }

        public bool ShowRightArrow { get; set; }

        //TEST
        private bool _showPlusSign;
        public bool ShowPlusSign
        { 
            get{ return _showPlusSign;}
            set
            {
                _showPlusSign = value;
            }
        }
        //public bool ShowPlusSign { get; set; }

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

			var textX = 8.ToPixels();

            if (ShowPlusSign || Icon.HasValue())
            {
				textX = 45.ToPixels();
            }

            if (!TextLine1.IsNullOrEmpty())
            {
                DrawText(canvas, TextLine1, textX, 21.ToPixels(), 16.ToPixels(), Typeface.DefaultBold, TextColorLine1);
                DrawText(canvas, TextLine2 ?? "", textX, 41.ToPixels(), 15.ToPixels(), Typeface.Default, TextColorLine2);
            }
            else
            {
                DrawText(canvas, TextLine2 ?? "", textX, 32.ToPixels(), 16.ToPixels(), Typeface.Default, TextColorLine2);
            }

            if (ShowRightArrow)
            {                
				canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, Resource.Drawable.right_arrow),
                    Width - (20.ToPixels()), 16.ToPixels(), null);
            }

            if (ShowPlusSign)
            {
                var bitmapIcon = BitmapFactory.DecodeResource (Resources, Resource.Drawable.add_list);								
				var offsetIcon = (canvas.Height - bitmapIcon.Height) / 2;
				canvas.DrawBitmap(bitmapIcon, 10.ToPixels(), offsetIcon, null);

            }
            else if (Icon.HasValue())
            {
                var identifier = Context.Resources.GetIdentifier(Icon, "drawable", Context.PackageName);

				var bitmapIcon = BitmapFactory.DecodeResource (Resources, identifier);									
				var offsetIcon = (canvas.Height - bitmapIcon.Height) / 2;
				canvas.DrawBitmap(bitmapIcon, 10.ToPixels(), offsetIcon,  null);
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