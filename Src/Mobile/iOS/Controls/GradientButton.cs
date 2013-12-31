using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using apcurium.MK.Booking.Mobile.Style;
using apcurium.MK.Common.Extensions;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register ("GradientButton")]
    public class GradientButton : UIButton
    {
        private bool _pressed;
        private UIColor[] _colors =
        {
            UIColor.FromRGB(240, 240, 240),
            UIColor.FromRGB(222, 222, 222),
            UIColor.FromRGB(200, 200, 200)
        };
		private UIColor[] _selectedColors =
		{
		    UIColor.FromRGB(240, 240, 240),
		    UIColor.FromRGB(222, 222, 222),
		    UIColor.FromRGB(200, 200, 200)
		};

        private UIRectCorner _roundedCorners = UIRectCorner.AllCorners;
		private float[] _colorLocations = { 0f, 0.93f, 1f };
		private float[] _selectedColorLocations = { 0f, 0.93f, 1f };
		private float _strokeLineWidth = 1f;
        private UIColor _strokeLineColor = UIColor.FromRGB(155, 155, 155) ;
        private UIColor _selectedStrokeLineColor;
        private float _cornerRadius = AppStyle.ButtonCornerRadius;
        private string _title = "";
        private CGColor _titleColor = UIColor.FromRGB(101, 101, 101).CGColor;
        private CGColor _selectedTitleColor;
        private readonly UIColor _highlightedColor = UIColor.FromRGBA(0f, 0f, 0f, 0.6f);
        private UIFont _titleFont = AppStyle.GetButtonFont( AppStyle.ButtonFontSize ) ;       
        private UIColor _textShadowColor;
        private UIColor _selectedTextShadowColor;
        private UIImage _image;
        private UIImage _rightImage;
        private UIImage _selectedImage;
        private ShadowDefinition _innerShadow;
        private ShadowDefinition _dropShadow;

        public GradientButton(IntPtr handle) : base(  handle )
        {
            Initialize();
        }

        public GradientButton(RectangleF rect): base(rect)
        {
            Initialize();
        }


        public GradientButton(RectangleF rect, float cornerRadius, ButtonStyle buttonStyle, string title, UIFont titleFont, string image = null) : base ( rect )
        {

			buttonStyle.TextShadowColor.Maybe( c => _textShadowColor = UIColor.FromRGBA(c.Red, c.Green, c.Blue, c.Alpha) );

            _colors = buttonStyle.Colors.Select ( color => UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha) ).ToArray();
			buttonStyle.SelectedColors.Maybe( () => {
				_selectedColors = buttonStyle.SelectedColors.Select ( color => UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha) ).ToArray();
				_selectedColorLocations = buttonStyle.SelectedColors.Select ( color => color.Location ).ToArray();
			});
			_colorLocations = buttonStyle.Colors.Select ( color => color.Location ).ToArray();
			_strokeLineColor = UIColor.FromRGBA( buttonStyle.StrokeColor.Red, buttonStyle.StrokeColor.Green, buttonStyle.StrokeColor.Blue, buttonStyle.StrokeColor.Alpha);
            _strokeLineWidth = buttonStyle.StrokeLineWidth;
            _cornerRadius = cornerRadius;
            _title = title ?? string.Empty;
            _titleColor = UIColor.FromRGBA( buttonStyle.TextColor.Red, buttonStyle.TextColor.Green, buttonStyle.TextColor.Blue, buttonStyle.TextColor.Alpha).CGColor;
			_titleFont = titleFont;
			_innerShadow = buttonStyle.InnerShadow;
			_dropShadow = buttonStyle.DropShadow;
            if (image != null)
            {
                _image = UIImage.FromFile(image);
            }

            Initialize();
        }

        private void Initialize ()
        {
            BackgroundColor = UIColor.Clear;
            Layer.MasksToBounds = false;
            ClipsToBounds = false;
            SetTitleColor( UIColor.Clear, UIControlState.Normal );
            SetTitleColor( UIColor.Clear, UIControlState.Highlighted);
            SetTitleColor( UIColor.Clear, UIControlState.Selected );
        }

        public void SetImage( string image )
        {
            if (image != null)
            {
                _image = UIImage.FromFile(image);
                SetNeedsDisplay();
            }
        }

        public void SetRightImage (string image)
        {
            if (image != null)
            {
                _rightImage = UIImage.FromFile (image);
                SetNeedsDisplay();
            }
        }

        public void SetSelectedImage( string image )
        {
            if (image != null)
            {
                _selectedImage = UIImage.FromFile(image);
                SetNeedsDisplay();
            }
        }

        public ShadowDefinition DropShadow
        {
            get{ return _dropShadow; }
            set
            {
                _dropShadow = value;
                SetNeedsDisplay();
            }
        }

        public ShadowDefinition InnerShadow
        {
            get{ return _innerShadow; }
            set
            {
                _innerShadow = value;
                SetNeedsDisplay();
            }
        }

        public UIColor StrokeLineColor
        {
            get{ return _strokeLineColor; }
            set
            {
                _strokeLineColor = value;
                SetNeedsDisplay();
            }
        }

        public UIColor SelectedStrokeLineColor
        {
            get{ return _selectedStrokeLineColor; }
            set
            {
                _selectedStrokeLineColor = value;
                SetNeedsDisplay();
            }
        }

        public float StrokeLineWidth
        {
            get{ return _strokeLineWidth; }
            set
            {
                _strokeLineWidth = value;
                SetNeedsDisplay();
            }
        }

        public float[] ColorLocations
        {
            get{ return _colorLocations; }
            set
            {
                _colorLocations = value;
                SetNeedsDisplay();
            }
        }

		public float[] SelectedColorLocations
		{
			get{ return _selectedColorLocations; }
			set
			{
				_selectedColorLocations = value;
				SetNeedsDisplay();
			}
		}

		public UIColor[] Colors
        {
            get{ return _colors; }
            set
            {
                _colors = value;
                SetNeedsDisplay();
            }
        }

		public UIColor[] SelectedColors
		{
			get{ return _selectedColors; }
			set
			{
				_selectedColors = value;
				SetNeedsDisplay();
			}
		}

        public float CornerRadius
        {
            get{ return _cornerRadius; }
            set
            {
                _cornerRadius = value;
                SetNeedsDisplay();
            }
        }
  
        public CGColor TitleColour
        {
            get{ return _titleColor;}
            set
            {
                _titleColor = value;
                SetNeedsDisplay();
            }
        }

        public CGColor SelectedTitleColour
        {
            get{ return _selectedTitleColor;}
            set
            {
                _selectedTitleColor = value;
                SetNeedsDisplay();
            }
        }


        public UIFont TitleFont
        {
            get{ return _titleFont;}
            set
            {
                _titleFont = value;
                SetNeedsDisplay();
            }
        }

        public UIColor TextShadowColor
        {
            get{ return _textShadowColor;}
            set
            {
                _textShadowColor = value;
                SetNeedsDisplay();
            }
        }

        public UIColor SelectedTextShadowColor
        {
            get{ return _selectedTextShadowColor;}
            set
            {
                _selectedTextShadowColor = value;
                SetNeedsDisplay();
            }
        }

        public override bool BeginTracking(UITouch uitouch, UIEvent uievent)
        {

            _pressed = true;
            SetNeedsDisplay();
            return base.BeginTracking(uitouch, uievent);
        }

        public override void EndTracking(UITouch uitouch, UIEvent uievent)
        {
            _pressed = false;
            SetNeedsDisplay();
            base.EndTracking(uitouch, uievent);
        }

        public override bool ContinueTracking(UITouch uitouch, UIEvent uievent)
        {
            var touch = uievent.AllTouches.AnyObject as UITouch;
            if (touch != null && Bounds.Contains (touch.LocationInView (this)))
                _pressed = true;
            else
                _pressed = false;

            SetNeedsDisplay();
            return base.ContinueTracking (uitouch, uievent);
        }

        public UIRectCorner RoundedCorners
        {
            get{ return _roundedCorners; }
            set
            {
                _roundedCorners = value;
                SetNeedsDisplay();
            }
        }

        public override void SetTitle(string title, UIControlState forState)
        {
            SetNeedsDisplay();
            _title = title ?? string.Empty;
        }

        public override void Draw (RectangleF rect)
        {
            base.Draw (rect);

            var colorSpace = CGColorSpace.CreateDeviceRGB ();
            var context = UIGraphics.GetCurrentContext ();

            var newGradientColors = Selected ? _selectedColors.Select (c => c.CGColor).ToArray () : _colors.Select (c => c.CGColor).ToArray ();
            var newGradientLocations = Selected ? _selectedColorLocations : _colorLocations;
            var newGradient = new CGGradient (colorSpace, newGradientColors, newGradientLocations);

            rect.Width -= _dropShadow != null ? Math.Abs (_dropShadow.OffsetX) : 0;
            rect.Height -= _dropShadow != null ? Math.Abs (_dropShadow.OffsetY) : 0;
            rect.X += _dropShadow != null && _dropShadow.OffsetX < 0 ? Math.Abs (_dropShadow.OffsetX) : 0;
            rect.Y += _dropShadow != null && _dropShadow.OffsetY < 0 ? Math.Abs (_dropShadow.OffsetY) : 0;


            var roundedRectanglePath = UIBezierPath.FromRoundedRect (rect, RoundedCorners, new SizeF (_cornerRadius, _cornerRadius));


            DrawBackground (rect, context, newGradient, roundedRectanglePath);
            DrawInnerShadow (context, roundedRectanglePath);
            DrawStroke (context, roundedRectanglePath, rect);
            var imageRect = DrawLeftImage (rect);
            var rightImageRect = DrawRightImage (rect);
            DrawText (rect, context, imageRect, rightImageRect);


            if (_pressed)
            {
                var insideRect = rect.Inset(0f, 0f);
                var container = UIBezierPath.FromRoundedRect(insideRect, RoundedCorners, new SizeF(  _cornerRadius , _cornerRadius ));
                context.SaveState();
                container.AddClip();

                _highlightedColor.SetFill();
                context.FillRect(insideRect);
                context.RestoreState();
            }
        }

        RectangleF DrawLeftImage (RectangleF rect)
        {
            var imageRect = new RectangleF ();
            if ((_image != null) && (!Selected || (_selectedImage == null))) {
                var emptySpaceX = rect.Width - _image.Size.Width;
                var emptySpaceY = rect.Height - _image.Size.Height;
                if (_title.IsNullOrEmpty ()) {
                    imageRect = new RectangleF (emptySpaceX / 2, emptySpaceY / 2, rect.Width - emptySpaceX, rect.Height - emptySpaceY);
                }
                else {
                    imageRect = new RectangleF (ContentEdgeInsets.Left + 3, emptySpaceY / 2, rect.Width - emptySpaceX, rect.Height - emptySpaceY);
                }
                _image.Draw (imageRect, CGBlendMode.Normal, 1f);
            }
            else
                if (Selected && (_selectedImage != null)) {
                    var emptySpaceX = rect.Width - _selectedImage.Size.Width;
                    var emptySpaceY = rect.Height - _selectedImage.Size.Height;
                    imageRect = new RectangleF (emptySpaceX / 2, emptySpaceY / 2, rect.Width - emptySpaceX, rect.Height - emptySpaceY);
                    _selectedImage.Draw (imageRect, CGBlendMode.Normal, 1f);
                }
            return imageRect;
        }

        RectangleF DrawRightImage (RectangleF rect)
        {
            var imageRect = new RectangleF();
            if (_rightImage != null) {
                var emptySpaceX = rect.Width - _rightImage.Size.Width;
                var emptySpaceY = rect.Height - _rightImage.Size.Height;
                imageRect = new RectangleF (emptySpaceX - ContentEdgeInsets.Right, emptySpaceY / 2, _rightImage.Size.Width, _rightImage.Size.Height);
                _rightImage.Draw (imageRect, CGBlendMode.Normal, 1f);
            }
            return imageRect;
        }

        protected virtual void DrawText (RectangleF rect, CGContext context, RectangleF imageRect, RectangleF rightImageRect)
        {
            context.SaveState ();
            var data = NSString.FromData(_title, NSStringEncoding.UTF8).Encode(NSStringEncoding.MacOSRoman);
            var dataBytes = new byte[data.Length];            
            Marshal.Copy(data.Bytes, dataBytes, 0, Convert.ToInt32(data.Length));
            context.SetFont(CGFont.CreateWithFontName(_titleFont.Name));
            context.SetFontSize(_titleFont.PointSize);
            context.SetTextDrawingMode (CGTextDrawingMode.Fill);
            var textColor = Selected ? (_selectedTitleColor ?? _titleColor) : _titleColor;
            var shadowColor = Selected ? _selectedTextShadowColor : _textShadowColor;
            context.SetStrokeColor (textColor);
            if (shadowColor != null) {
                context.SetShadowWithColor (new SizeF (0f, -0.5f), 0.5f, shadowColor.CGColor);
            }
            context.SetFillColor (textColor);
            context.TextMatrix = new CGAffineTransform (1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f);
            var titleSize = ((NSString)_title).StringSize (_titleFont);
            if (HorizontalAlignment == UIControlContentHorizontalAlignment.Left) {
                context.ShowTextAtPoint (ContentEdgeInsets.Left + imageRect.Left + imageRect.Width + 10, rect.GetMidY () + (_titleFont.PointSize / 3), dataBytes);

            } else {
                context.ShowTextAtPoint (((rect.Width - imageRect.Width) / 2) - (titleSize.Width / 2) + imageRect.Right, rect.GetMidY () + (_titleFont.PointSize / 3), dataBytes);
            }
            context.RestoreState ();

            if (Selected) {
            }
        }

        void DrawInnerShadow (CGContext context, UIBezierPath roundedRectanglePath)
        {
            if (_innerShadow != null) {
                var roundedRectangleBorderRect = roundedRectanglePath.Bounds;
                roundedRectangleBorderRect.Inflate (_innerShadow.BlurRadius, _innerShadow.BlurRadius);
                roundedRectangleBorderRect.Offset (-_innerShadow.OffsetX, -_innerShadow.OffsetY);
                roundedRectangleBorderRect = RectangleF.Union (roundedRectangleBorderRect, roundedRectanglePath.Bounds);
                roundedRectangleBorderRect.Inflate (1, 1);
                var roundedRectangleNegativePath = UIBezierPath.FromRect (roundedRectangleBorderRect);
                roundedRectangleNegativePath.AppendPath (roundedRectanglePath);
                roundedRectangleNegativePath.UsesEvenOddFillRule = true;
                context.SaveState ();
                {
                    var xOffset = _innerShadow.OffsetX + (float)Math.Round (roundedRectangleBorderRect.Width);
                    var yOffset = _innerShadow.OffsetY;
                    context.SetShadowWithColor (new SizeF (xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)), _innerShadow.BlurRadius, UIColor.FromRGBA (_innerShadow.Color.Red, _innerShadow.Color.Green, _innerShadow.Color.Blue, _innerShadow.Color.Alpha).CGColor);
                    roundedRectanglePath.AddClip ();
                    var transform = CGAffineTransform.MakeTranslation (-(float)Math.Round (roundedRectangleBorderRect.Width), 0);
                    roundedRectangleNegativePath.ApplyTransform (transform);
                    UIColor.Gray.SetFill ();
                    roundedRectangleNegativePath.Fill ();
                }
                context.RestoreState ();
            }
        }

        void DrawBackground (RectangleF rect, CGContext context, CGGradient newGradient, UIBezierPath roundedRectanglePath)
        {
            context.SaveState ();
            if (_dropShadow != null) {
                context.SetShadowWithColor (new SizeF (_dropShadow.OffsetX, _dropShadow.OffsetY), _dropShadow.BlurRadius, UIColor.FromRGBA (_dropShadow.Color.Red, _dropShadow.Color.Green, _dropShadow.Color.Blue, _dropShadow.Color.Alpha).CGColor);
            }

            context.BeginTransparencyLayer (null);
            roundedRectanglePath.AddClip ();
            context.DrawLinearGradient (newGradient, new PointF (rect.X + (rect.Width / 2.0f), rect.Y), new PointF (rect.X + (rect.Width / 2.0f), rect.Y + rect.Height), 0);
            context.EndTransparencyLayer ();
            context.RestoreState ();
        }

        protected virtual void DrawStroke (CGContext context, UIBezierPath roundedRectanglePath, RectangleF rect)
        {
            context.SaveState ();

            roundedRectanglePath.LineWidth = _strokeLineWidth;
            var strokeLineColor = Selected ? (_selectedStrokeLineColor ?? _strokeLineColor) : _strokeLineColor;
            strokeLineColor.SetStroke ();
            roundedRectanglePath.AddClip ();
            context.AddPath (roundedRectanglePath.CGPath);
            context.StrokePath ();
            context.RestoreState ();
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
                base.UserInteractionEnabled = value;
                base.Alpha = value ? 1f : 0.6f;
            }
        }
    }
}

