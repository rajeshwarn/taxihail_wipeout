using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace TaxiMobile.Controls
{
    public class BaloonLayout : LinearLayout
    {
        	
		public BaloonLayout (Context context) : base( context )
		{
		}
		
		public BaloonLayout (Context context, IAttributeSet attrs) : base( context, attrs )
		{
		}

        public BaloonLayout(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
            : base(ptr, handle)
		{
			
		}

        protected override void DispatchDraw(Android.Graphics.Canvas canvas)
        {
            Paint panelPaint = new Paint();
            panelPaint.SetARGB(0, 0, 0, 0);

            RectF panelRect = new RectF();
            
            panelRect.Set( 0, 0, MeasuredWidth, MeasuredHeight);
            canvas.DrawRoundRect(panelRect, 5, 5, panelPaint);

            RectF baloonRect = new RectF();
            baloonRect.Set(0, 0, MeasuredWidth, 2 * (MeasuredHeight / 3));
            panelPaint.SetARGB(230, 255, 255, 255);
            canvas.DrawRoundRect(baloonRect, 10, 10, panelPaint);

            Path baloonTip = new Path();
            baloonTip.MoveTo(5 * (MeasuredWidth / 8), 2 * (MeasuredHeight / 3));
            baloonTip.LineTo(MeasuredWidth / 2, MeasuredHeight);
            baloonTip.LineTo(3 * (MeasuredWidth / 4), 2 * (MeasuredHeight / 3));

            canvas.DrawPath(baloonTip, panelPaint);
            
            base.DispatchDraw(canvas);
        }
		
    }
}