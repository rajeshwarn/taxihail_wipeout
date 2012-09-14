using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Graphics;
using Android.Text;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class ListViewCell2 : TextView
    {
		public ListViewCell2(Context context)
            : base(context)
        {
        }

		public ListViewCell2(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

		public ListViewCell2(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
            : base(ptr, handle)
        {
        }

        public string TextLine1 { get; set; }
		public string TextLine2 { get; set; }

		public bool IsTop { get; set; }
		public bool IsBottom { get; set; }

        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);

            DrawText(canvas, TextLine1 ?? "", 8, 35, 20, AppFonts.Bold);
			DrawText(canvas, TextLine2 ?? "", 8, 57, 16, AppFonts.Regular);

			var d = IsTop ? Resource.Drawable.cell_top : IsBottom ? Resource.Drawable.cell_bottom : Resource.Drawable.cell_middle;
			SetBackgroundDrawable( Resources.GetDrawable( d ) );

//			if( IsTop )
//			{
//				Paint paint = new Paint(PaintFlags.AntiAlias);            
//				paint.StrokeWidth = 1;
//				paint.Color = Android.Graphics.Color.Black;
//				paint.SetStyle(Android.Graphics.Paint.Style.Stroke);
//				paint.AntiAlias = true;
//				var path = new Path();
//				path.Reset();
//				path.SetFillType(Path.FillType.InverseEvenOdd);      
//				path.MoveTo(0,canvas.Height-1 );
//				path.LineTo(0, 0);
//				path.LineTo(canvas.Width-1, 0);
//				path.LineTo(canvas.Width-1, canvas.Height - 1);
//				path.LineTo(0, canvas.Height -1 );
//				canvas.DrawPath(path, paint);
//
//				paint = new Paint(PaintFlags.AntiAlias); 
//				paint.Color = Android.Graphics.Color.White;
//				paint.SetStyle(Android.Graphics.Paint.Style.Fill);
//				canvas.DrawPath(path, paint);
//
//			}


        }

        private void DrawText(Android.Graphics.Canvas canvas, string text, float x, float y, float textSize, Typeface typeface)
        {
            TextPaint paintText = new TextPaint(PaintFlags.AntiAlias | Android.Graphics.PaintFlags.LinearText);
            var rect = new Rect();
            paintText.TextSize = textSize;
            paintText.GetTextBounds(text, 0, text.Length, rect);
            paintText.SetARGB(255, 49, 49, 49);
			paintText.SetTypeface(typeface);
            canvas.DrawText(text, x, y, paintText);

        }

    }
}