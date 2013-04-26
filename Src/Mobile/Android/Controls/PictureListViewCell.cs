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
using apcurium.MK.Booking.Mobile.Client.Helpers;

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
			
			var leftTextX = 70.ToPixels();
			var leftTextY = 30.ToPixels();
			var textSize = 17.ToPixels();
			var leftTextWidth = 75.ToPixels();

            if (!string.IsNullOrEmpty(TextLeft))
			{

				DrawText(canvas, TextLeft ?? "", leftTextX, leftTextY, textSize, AppFonts.Bold);
				DrawText(canvas, TextRight ?? "", leftTextX + leftTextWidth, leftTextY, textSize, AppFonts.Regular);
			}
			else
			{
				DrawText(canvas, TextRight ?? "",leftTextX, leftTextY, textSize, AppFonts.Regular);
			}

            if (ShowAddSign)
            {
				canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, Resource.Drawable.add_btn), 4.ToPixels(), 7.ToPixels(), null);
            }

            if (!string.IsNullOrEmpty(Picture))
            {
                var resource = Resources.GetIdentifier(Picture.ToLower(), "drawable", Context.PackageName);
                if (resource != 0)
                {
					canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, resource), 0.ToPixels(), 3.ToPixels(), null);
                }
            }

            var d = IsTop && !IsBottom 
						? Resource.Drawable.cell_top_state 
						: IsBottom && !IsTop 
							? Resource.Drawable.cell_bottom_state 
							: IsTop && IsBottom 
								? Resource.Drawable.cell_full_state 
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
			var ellipsizedText = TextUtils.Ellipsize(text, p, (this.Width - 45).ToPixels(), TextUtils.TruncateAt.End);
            if (string.IsNullOrEmpty(ellipsizedText))
            {
                ellipsizedText = text;
            }


            canvas.DrawText(ellipsizedText, x, y, paintText);

        }

    }
}