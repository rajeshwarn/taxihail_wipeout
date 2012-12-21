using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class PictureListViewCell: TextView
    {
		public PictureListViewCell(Context context)
            : base(context)
        {
        }

		public PictureListViewCell(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public PictureListViewCell(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
            : base(ptr, handle)
        {
        }




        public string TextLeft { get; set; }
        public string TextRight { get; set; }
        public string Picture { get;set; }

		public bool IsTop { get; set; }
		public bool IsBottom { get; set; }
        public bool ShowAddSign { get; set; }

        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);
            if (!string.IsNullOrEmpty(TextLeft))
			{
                DrawText(canvas, TextLeft ?? "", 100, 45, 20, AppFonts.Bold);
                DrawText(canvas, TextRight ?? "", 220, 45, 20, AppFonts.Regular);
			}
			else
			{
                DrawText(canvas, TextRight ?? "",100, 45, 20, AppFonts.Regular);
			}

            if (ShowAddSign)
            {
                //canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, Resource.Drawable.add_btn), 10, 10, null);
            }

            if (!string.IsNullOrEmpty(Picture))
            {
                var resource = Resources.GetIdentifier(Picture.ToLower(), "drawable", Context.PackageName);
                if (resource != 0)
                {
                    canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, resource), 0, 6, null);
                }
            }

            var d = IsTop && !IsBottom 
						? Resource.Drawable.cell_top_state 
						: IsBottom && !IsTop 
							? Resource.Drawable.blank_bottom_state 
							: IsTop && IsBottom 
								? Resource.Drawable.blank_single_state 
								:  Resource.Drawable.cell_middle_state;

			SetBackgroundDrawable( Resources.GetDrawable( d ) );
        }

        private void DrawText(Android.Graphics.Canvas canvas, string text, float x, float y, float textSize, Typeface typeface)
        {
            TextPaint paintText = new TextPaint(PaintFlags.AntiAlias | Android.Graphics.PaintFlags.LinearText);
            var rect = new Rect();
            paintText.TextSize = textSize;
            paintText.GetTextBounds(text, 0, text.Length, rect);
            paintText.SetARGB(255, 49, 49, 49);
			paintText.SetTypeface(typeface);

            var p = new TextPaint();
            p.SetTypeface(typeface);

            p.TextSize = textSize;
            var ellipsizedText = TextUtils.Ellipsize(text, p, this.Width - 45, TextUtils.TruncateAt.End);
            if (string.IsNullOrEmpty(ellipsizedText))
            {
                ellipsizedText = text;
            }


            canvas.DrawText(ellipsizedText, x, y, paintText);

        }

    }
}