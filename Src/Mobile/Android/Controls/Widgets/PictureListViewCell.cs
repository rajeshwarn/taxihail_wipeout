using System;
using System.Collections;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class PictureListViewCell : TextView
    {
        public string TextLeft { get; set; }
        public string TextRight { get; set; }
        public string Picture { get; set; }
        public bool ShowAddSign { get; set; }
        public bool IsBottom { get; set; }
		private Android.Graphics.Drawables.Drawable _backgroundDrawable;	
		private Hashtable pictureTable = new Hashtable ();

        public PictureListViewCell(Context context)
            : base(context)
        {
        }

        public PictureListViewCell(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public PictureListViewCell(IntPtr ptr, JniHandleOwnership handle)
            : base(ptr, handle)
        {
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            var imageLeftX = 8.ToPixels();
            var imageLeftY = 8.ToPixels();
            var imageLeftSize = 50.ToPixels();

            var textY = 31.ToPixels();
            var leftTextX = (imageLeftX + imageLeftSize) + 10.ToPixels();

            var textFontSize = 17.ToPixels();

            var imageButtonRightSize = 47.ToPixels();
            var textWidth = 75.ToPixels();

			var imageWidth = 0;

			if (ShowAddSign) {

				var identifier = Resource.Drawable.add_list;

				if (pictureTable [identifier] == null) {
					pictureTable [identifier] = BitmapFactory.DecodeResource (Resources, identifier);                              
				}


				if (this.Services ().Localize.IsRightToLeft) {
					imageWidth = ((Bitmap)pictureTable [identifier]).Width;
					imageLeftX = Width - 12.ToPixels () - imageWidth;
				}
				canvas.DrawBitmap ((Bitmap)pictureTable [identifier], imageLeftX , imageLeftY + 7.ToPixels (), null);

			} else if (!string.IsNullOrEmpty (Picture)) {

				var identifier = Resources.GetIdentifier (Picture.ToLower (), "drawable", Context.PackageName);

				if (pictureTable [identifier] == null && identifier != 0) {
					pictureTable.Add (identifier, BitmapFactory.DecodeResource (Resources, identifier));
				} else {

					if (this.Services ().Localize.IsRightToLeft) {
						imageWidth = ((Bitmap)pictureTable [identifier]).Width;
						imageLeftX = Width - imageLeftX - imageWidth;
					}
					canvas.DrawBitmap ((Bitmap)pictureTable [identifier], imageLeftX, imageLeftY, null);
				}
			}

			DrawText(canvas, TextLeft ?? "", leftTextX, textY, textFontSize, ShowAddSign ? Typeface.DefaultBold : Typeface.Default, imageWidth);

            if (!string.IsNullOrEmpty(TextRight))
				DrawText(canvas, TextRight ?? "", canvas.Width - (textWidth + imageButtonRightSize), textY, textFontSize, Typeface.DefaultBold, imageWidth, true);

			if (_backgroundDrawable == null) {
				_backgroundDrawable = Resources.GetDrawable (IsBottom 
					? Resource.Drawable.cell_bottom_state 
					: Resource.Drawable.cell_middle_state);
				SetBackgroundDrawable(_backgroundDrawable);
			}	
        }

        private void DrawText(Canvas canvas, string text, float x, float y, float textSize,
			Typeface typeface, float imageWith = 0, bool isRightText = false)
        {
            var paintText = new TextPaint(PaintFlags.AntiAlias | PaintFlags.LinearText);
			var textRect = new Rect();
            paintText.TextSize = textSize;
			paintText.GetTextBounds(text, 0, text.Length, textRect);
            paintText.SetARGB(255, 49, 49, 49);
            paintText.SetTypeface(typeface);

            var p = new TextPaint();
            p.SetTypeface(typeface);

            p.TextSize = textSize;
            var ellipsizedText = TextUtils.Ellipsize(text, p, (Width - 45).ToPixels(), TextUtils.TruncateAt.End);
            if (string.IsNullOrEmpty(ellipsizedText))
            {
                ellipsizedText = text;
            }

			if (this.Services().Localize.IsRightToLeft 
				&& !isRightText) {
				x = canvas.Width - textRect.Width() - imageWith - 15.ToPixels();
			}

			if (this.Services().Localize.IsRightToLeft 
				&& isRightText) {
				x = 15.ToPixels();
			}

            canvas.DrawText(ellipsizedText, x, y, paintText);
        }
    }
}