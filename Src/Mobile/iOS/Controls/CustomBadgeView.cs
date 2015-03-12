using System;
using UIKit;
using CoreGraphics;
using Foundation;
using CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class CustomBadgeView : UIView
	{
		private string _text;
		private UIColor _textColor;
		private UIColor _badgeColor;
		private bool _autoResize;
		private float _cornerRoundness;
		private float _scaleFactor;

		public CustomBadgeView (string text)
			: this(text, true) 
		{ 
		}

		public CustomBadgeView (string text, bool autoResize)
			: this(text, autoResize, 35, -5)
		{
		}
		public CustomBadgeView (string text, nfloat xOffset, nfloat yOffset)
			: this(text, true, xOffset, yOffset)
		{
		}

		public CustomBadgeView (string text, bool autoResize, nfloat xOffset, nfloat yOffset)
			: base(new CGRect(xOffset, yOffset, 25, 25))
		{
			XOffset = xOffset;
			ContentScaleFactor = UIScreen.MainScreen.Scale;
			BackgroundColor = UIColor.Clear;
			_text = text;
			_textColor = UIColor.White;
            _badgeColor = UIColor.FromRGB(252, 33, 37);
			_cornerRoundness = 0.4f;
			_scaleFactor = 1.0f;
			_autoResize = autoResize;
			if (autoResize)
			{
				AutoResizeBadge();
			}
			base.UserInteractionEnabled = false;

		}

		public string Text {
			get {
				return _text;
			}
			set {
				if (_text != value) {
					_text = value;
					Redraw ();
				}
			}
		}

		public nfloat XOffset
		{
			get;
			set;
		}

		public nfloat YOffset
		{
			get;
			set;
		}

		public UIColor TextColor {
			get {
				return _textColor;
			}
			set {
				if (_textColor != value) {
					_textColor = value;
					Redraw (false);
				}
			}
		}

		public UIColor BadgeColor {
			get {
				return _badgeColor;
			}
			set {
				if (_badgeColor != value) {
					_badgeColor = value;
					Redraw (false);
				}
			}
		}		

		public bool AutoResize {
			get 
			{
				return _autoResize;
			}
			set 
			{
				_autoResize = value;
			}
		}

		public float CornerRoundness {
			get {
				return _cornerRoundness;
			}
			set {
				if (_cornerRoundness != value) {
					_cornerRoundness = value;
					Redraw ();
				}
			}
		}

		public float ScaleFactor {
			get {
				return _scaleFactor;
			}
			set {
				if (_scaleFactor != value) {
					_scaleFactor = value;
					Redraw ();
				}
			}
		}

		private void Redraw (bool autoResize = true)
		{
			if (autoResize && _autoResize) {
				AutoResizeBadge ();
				return; // AutoResize calls redraw
			}
			this.SetNeedsDisplay ();
		}

		public void AutoResizeBadge ()
		{
			var stringSize = new NSString (this._text).StringSize (UIFont.BoldSystemFontOfSize (12));
			nfloat flexSpace, rectWidth, rectHeight;
			var frame = this.Frame;
			if (this._text.Length >= 2) {
				flexSpace = this._text.Length;
                rectWidth = (nfloat)(25 + (stringSize.Width + flexSpace));
				rectHeight = 25;
				frame.Width = rectWidth * this._scaleFactor;
				frame.Height = rectHeight * this._scaleFactor;
				frame.X = XOffset - stringSize.Width  ;

			} else {
				frame.Width = 25 * this._scaleFactor;
				frame.Height = 25 * this._scaleFactor;
				frame.X = XOffset;
			}
			this.Frame = frame;
			this.Redraw (false);
		}

		public override void Draw (CGRect rect)
		{
			var context = UIGraphics.GetCurrentContext ();
			this.DrawRoundedRect (context, rect);			

			if (!string.IsNullOrEmpty(_text)) {
				_textColor.SetColor ();
				var sizeOfFont = 13.5f * _scaleFactor;
				if (_text.Length < 2) {
					sizeOfFont += sizeOfFont * 0.20f;
				}
				var font = UIFont.BoldSystemFontOfSize (sizeOfFont);
				var text = new NSString (this._text);
				var textSize = text.StringSize (font);
				var textPos = new CGPoint (rect.Width / 2 - textSize.Width / 2, rect.Height / 2 - textSize.Height / 2);
				if (_text.Length < 2)
				{
					textPos.X += 0.5f;
				}
				text.DrawString (textPos, font);
			}
		}

		private nfloat MakePath (CGContext context, CGRect rect)
		{
			var radius = rect.Bottom * this._cornerRoundness;
			var puffer = rect.Bottom * 0.12f;
			var maxX = rect.Right - (puffer * 2f);
			var maxY = rect.Bottom - puffer;
			var minX = rect.Left + (puffer * 2f);
			var minY = rect.Top + puffer;
			if (maxX - minX < 20f) {
				maxX = rect.Right - puffer;
				minX = rect.Left + puffer;
			}

			context.AddArc (maxX - radius, minY + radius, radius, (float)(Math.PI + (Math.PI / 2)), 0f, false);
			context.AddArc (maxX - radius, maxY - radius, radius, 0, (float)(Math.PI / 2), false);
			context.AddArc (minX + radius, maxY - radius, radius, (float)(Math.PI / 2), (float)Math.PI, false);
			context.AddArc (minX + radius, minY + radius, radius, (float)Math.PI, (float)(Math.PI + Math.PI / 2), false);

			return maxY;
		}

		private void DrawRoundedRect (CGContext context, CGRect rect)
		{
			context.SaveState ();

			context.BeginPath ();
			context.SetFillColor (_badgeColor.CGColor);
			MakePath (context, rect);
			context.FillPath ();
			context.RestoreState ();
		}		

		private nfloat[] GetComponents (UIColor[] colors)
		{
			var ret = new nfloat[colors.Length * 4];
			for (int i = 0; i < colors.Length; i++) {
				var f = i * 4;
				colors [i].GetRGBA (out ret [f], out ret [f + 1], out ret [f + 2], out ret [f + 3]);
			}
			return ret;
		}

		
	}
}

