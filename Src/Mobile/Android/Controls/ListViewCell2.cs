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
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Helpers;

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


        public Color TextColor { get; set; }

        private bool _isTop;
        
        public bool IsTop { 
            get{ return _isTop;} 
            set
            { 
                _isTop = value; 
                Invalidate();
                
            } 
        }
        
        private bool _isBottom;
        public bool IsBottom { get{ return _isBottom;} 
            set
            { 
                _isBottom = value; 
                Invalidate();

            } 
        }
        
        public bool ShowRightArrow { get; set; }
        public bool ShowPlusSign { get; set; }
        
        public string Icon { get; set; }
        
        
        
        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            var gray = new Color(86, 86, 86, 255);
            var darkGray = new Color(50, 50, 50, 255);

            var textX =  ShowPlusSign ? 65.ToPixels() : 8.ToPixels();
            
            if ( !ShowPlusSign &&( Icon.HasValue() ))
            {
                textX =   40.ToPixels() ;
            }
            
            if( !TextLine1.IsNullOrEmpty() )
            {
                DrawText(canvas, TextLine1 ?? "", textX, 21.ToPixels(), 16.ToPixels(), AppFonts.Bold,darkGray);
                DrawText(canvas, TextLine2 ?? "", textX,  41.ToPixels(),15.ToPixels(), AppFonts.Regular,gray );
            }
            else
            {
                DrawText(canvas, TextLine2 ?? "", textX,  32.ToPixels(), 16.ToPixels(), AppFonts.Regular,gray );
            }
            
            if (ShowRightArrow)
            {
                canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, Resource.Drawable.right_arrow), Width-(20.ToPixels()), 16.ToPixels(), null);
            }
            
            if (ShowPlusSign)
            {

                canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, Resource.Drawable.add_btn), 6.ToPixels(),10.ToPixels(), null);
            }
            else if ( Icon.HasValue() )
            {
                var identifier = Context.Resources.GetIdentifier(  Icon, "drawable", Context.PackageName);

                
                canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, identifier ), 6.ToPixels(), 10.ToPixels(), null);
            }
            
            
            var d = IsTop && !IsBottom 
                ? Resource.Drawable.cell_top_state 
                : IsBottom && !IsTop 
                    ? Resource.Drawable.cell_bottom_state 
                    : IsTop && IsBottom 
                        ? Resource.Drawable.cell_full_state 
                        : Resource.Drawable.cell_middle_state;

            SetBackgroundDrawable( Resources.GetDrawable( d ) );
            
            
            
            
        }
        
        private void DrawText(Canvas canvas, string text, float x, float y, float textSize, Typeface typeface, Color color )
        {
            var paintText = new TextPaint(PaintFlags.AntiAlias | PaintFlags.LinearText);
            var rect = new Rect();
            paintText.TextSize = textSize;
            paintText.GetTextBounds(text, 0, text.Length, rect);
            paintText.SetARGB(color.A, color.R  , color.G , color.B );
            paintText.SetTypeface(typeface);
            var p = new TextPaint();
            p.SetTypeface(typeface);
            
            p.TextSize = textSize;
            var ellipsizedText = TextUtils.Ellipsize(text, p, this.Width - 75.ToPixels(), TextUtils.TruncateAt.End);
            if (ellipsizedText.IsNullOrEmpty())
            {
                ellipsizedText = text;
            }
            
            canvas.DrawText(ellipsizedText, x, y, paintText);
            
        }
    }
}