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
    public class TextProgressButton : ViewGroup
    {
         public TextProgressButton(Context context)
            : base(context)
        {
        }

        public TextProgressButton(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public TextProgressButton(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
            : base(ptr, handle)
        {
        }
        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
        
        }

        public string TextLine1 { get; set; }

        public string TextLine2 { get; set; }

        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);


            DrawText(canvas, TextLine1 ?? "", 8, 20, 15);
            DrawText(canvas, TextLine2 ?? "", 8, 45, 20);


        }

        private void DrawText(Android.Graphics.Canvas canvas, string text, float x, float y, float textSize)
        {
            TextPaint paintText = new TextPaint(PaintFlags.AntiAlias | Android.Graphics.PaintFlags.LinearText);
            var rect = new Rect();
            paintText.TextSize = textSize;
            paintText.GetTextBounds(text, 0, text.Length, rect);
            paintText.SetARGB(255, 49, 49, 49);
            paintText.SetTypeface(AppFonts.Regular);
            canvas.DrawText(text, x, y, paintText);

        }


        //public string TextLine2 
        //{
        //    get
        //    {
        //        return Button.TextLine2;
        //    }
        //    set { Button.TextLine2 = value; } 
        //}
        
        //    public TextButton Button { get; set; }
        
        //private void Initialize()
        //{
        //    if (Button == null)
        //    {
                
        //        Button = new TextButton(Context);

        //        this.AddView(Button);
        //    }
        //}

    }
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

        public TextButton(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
            : base(ptr, handle)
        {


        }



        public string TextLine1 { get; set; }

        public string TextLine2 { get; set; }

        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);


            DrawText(canvas, TextLine1 ?? "", 8, 20, 15);
            DrawText(canvas, TextLine2 ?? "", 8, 45, 20);


        }

        private void DrawText(Android.Graphics.Canvas canvas, string text, float x, float y, float textSize)
        {
            TextPaint paintText = new TextPaint(PaintFlags.AntiAlias | Android.Graphics.PaintFlags.LinearText);
            var rect = new Rect();
            paintText.TextSize = textSize;
            paintText.GetTextBounds(text, 0, text.Length, rect);
            paintText.SetARGB(255, 49, 49, 49);
            paintText.SetTypeface(AppFonts.Regular);
            canvas.DrawText(text, x, y, paintText);

        }

    }
}