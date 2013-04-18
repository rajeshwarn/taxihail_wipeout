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
        
        
        
        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);
            var textX =  ShowPlusSign ? DrawHelper.GetPixels(65) : DrawHelper.GetPixels(8);
            
            if ( !ShowPlusSign &&( Icon.HasValue() ))
            {
                textX =   DrawHelper.GetPixels(40) ;
            }
            
            if( !TextLine1.IsNullOrEmpty() )
            {
                DrawText(canvas, TextLine1 ?? "", textX, DrawHelper.GetPixels( 21 ), DrawHelper.GetPixels( 16 ), AppFonts.Bold,new Color( 50,50,50,255 ));
                DrawText(canvas, TextLine2 ?? "", textX,  DrawHelper.GetPixels( 41 ),  DrawHelper.GetPixels( 15 ), AppFonts.Regular,new Color( 86,86,86,255 ) );
            }
            else
            {
                DrawText(canvas, TextLine2 ?? "", textX, DrawHelper.GetPixels( 32 ), DrawHelper.GetPixels(16), AppFonts.Regular,new Color( 86,86,86,255 ) );
            }
            
            if (ShowRightArrow)
            {
                canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, Resource.Drawable.right_arrow), this.Width-DrawHelper.GetPixels(20), DrawHelper.GetPixels(16), null);
            }
            
            if (ShowPlusSign)
            {
                canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, Resource.Drawable.add_btn), DrawHelper.GetPixels(6), DrawHelper.GetPixels(10), null);
            }
            else if ( Icon.HasValue() )
            {
                var identifier = Context.Resources.GetIdentifier(  Icon, "drawable", Context.PackageName);
                //return _context.Resources.GetString(identifier);
                
                canvas.DrawBitmap(BitmapFactory.DecodeResource(Resources, identifier ), DrawHelper.GetPixels(6), DrawHelper.GetPixels(10), null);
            }
            
            
            
            var d = IsTop && !IsBottom ? Resource.Drawable.cell_top_state : IsBottom && !IsTop ? Resource.Drawable.cell_bottom_state : IsTop && IsBottom ? Resource.Drawable.cell_full_state : Resource.Drawable.cell_middle_state;
            SetBackgroundDrawable( Resources.GetDrawable( d ) );
            
            
            
            
        }
        
        private void DrawText(Android.Graphics.Canvas canvas, string text, float x, float y, float textSize, Typeface typeface, Color color )
        {
            TextPaint paintText = new TextPaint(PaintFlags.AntiAlias | Android.Graphics.PaintFlags.LinearText);
            var rect = new Rect();
            paintText.TextSize = textSize;
            paintText.GetTextBounds(text, 0, text.Length, rect);
            paintText.SetARGB(color.A, color.R  , color.G , color.B );
            paintText.SetTypeface(typeface);
            var p = new TextPaint();
            p.SetTypeface(typeface);
            
            p.TextSize = textSize;
            var ellipsizedText = TextUtils.Ellipsize(text, p, this.Width - DrawHelper.GetPixels(75), TextUtils.TruncateAt.End);
            if (ellipsizedText.IsNullOrEmpty())
            {
                ellipsizedText = text;
            }
            
            canvas.DrawText(ellipsizedText, x, y, paintText);
            
        }
    }
}