using System;
using System.Collections;
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

		private Android.Graphics.Drawables.Drawable _backgroundDrawable;	

		private Hashtable pictureTable = new Hashtable ();
		        
        private bool _showPlusSign;
        public bool ShowPlusSign
        { 
            get{ return _showPlusSign;}
            set
            {
                _showPlusSign = value;
            }
        }        

        public string Icon { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global

        public Color TextColorLine1 { get; set; }

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
				var identifier = Resource.Drawable.add_list;

				if (pictureTable[identifier] == null) {
					pictureTable[identifier] = BitmapFactory.DecodeResource (Resources, Resource.Drawable.right_arrow);
				}					

				canvas.DrawBitmap((Bitmap)pictureTable[identifier],
					Width - (20.ToPixels()), 16.ToPixels(), null);
			}

			if (ShowPlusSign)
			{
				var identifier = Resource.Drawable.add_list;

				if (pictureTable[identifier] == null) {
					pictureTable[identifier] = BitmapFactory.DecodeResource (Resources, identifier);								
				}

				var offsetIcon = (canvas.Height - ((Bitmap)pictureTable[identifier]).Height) / 2;

				var xPlus = 10.ToPixels ();
				if (this.Services ().Localize.IsRightToLeft) {
					xPlus = Width - (ShowRightArrow ? 40 : 25).ToPixels ();
				}

				canvas.DrawBitmap((Bitmap)pictureTable[identifier], xPlus, offsetIcon, null);

			}
			else if (Icon.HasValue())
			{
				var identifier = Context.Resources.GetIdentifier(Icon, "drawable", Context.PackageName);

				if (pictureTable[identifier] == null) {
					pictureTable[identifier] = BitmapFactory.DecodeResource (Resources, identifier);                              
				}

				var xIcon = 10.ToPixels ();
				if (this.Services ().Localize.IsRightToLeft) {
					xIcon = Width - xIcon - (ShowRightArrow ? 45.ToPixels() : 0);
				}
				var offsetIcon = (canvas.Height - ((Bitmap)pictureTable[identifier]).Height) / 2;
				canvas.DrawBitmap((Bitmap)pictureTable[identifier], xIcon, offsetIcon,  null);
			}

			if (_backgroundDrawable == null) {
				_backgroundDrawable = Resources.GetDrawable (IsBottom 
					? Resource.Drawable.cell_bottom_state 
					: Resource.Drawable.cell_middle_state);
				SetBackgroundDrawable(_backgroundDrawable);
			}	
        }

        private void DrawText(Canvas canvas, string text, float x, float y, float textSize, Typeface typeface,
            Color color)
        {
            var paintText = new TextPaint(PaintFlags.LinearText | PaintFlags.AntiAlias)
            {
                TextSize = textSize
            };

			var textRect = new Rect ();
			paintText.GetTextBounds (text, 0, text.Length, textRect);
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

			if (this.Services().Localize.IsRightToLeft) {
				x = canvas.Width - 75 - textRect.Width() - (ShowRightArrow ? 35.ToPixels() : 0);
			}

            canvas.DrawText(ellipsizedText, x, y, paintText);
        }
    }
}