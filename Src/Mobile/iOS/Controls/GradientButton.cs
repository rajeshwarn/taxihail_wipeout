using System;
using System.Linq;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;


namespace apcurium.MK.Booking.Mobile.Client
{
    [Register ("GradientButton")]
    public class GradientButton : UIButton
    {
        private bool _pressed;
        private UIColor[] _colors = new UIColor[]
            {
                UIColor.FromRGB(240, 240, 240),
                UIColor.FromRGB(222, 222, 222),
                UIColor.FromRGB(200, 200, 200)
            };
        private float[] _colorLocations = new float[] { 0f, 0.93f, 1f };
        private float _strokeLineWidth = 1f;
        private UIColor _strokeLineColor = UIColor.FromRGB(155, 155, 155) ;
        private float _cornerRadius = AppStyle.ButtonCornerRadius;
        private string _title = "";
        private CGColor _titleColor = UIColor.FromRGB(101, 101, 101).CGColor;
        private UIColor _highlightedColor = UIColor.FromRGBA(0f, 0f, 0f, 0.2f);
        private UIFont _titleFont = AppStyle.GetButtonFont( AppStyle.ButtonFontSize ) ;       
        private UIColor _textShadowColor = null;
        private UIImage _image;
        private ShadowSetting _innerShadow;
        private ShadowSetting _dropShadow;

        public GradientButton(IntPtr handle) : base(  handle )
        {

            BackgroundColor = UIColor.Clear;
            Layer.MasksToBounds = false;
            ClipsToBounds = false;
            SetTitleColor( UIColor.Clear, UIControlState.Normal );
            SetTitleColor( UIColor.Clear, UIControlState.Highlighted);
            SetTitleColor( UIColor.Clear, UIControlState.Selected );
        }

        public GradientButton(RectangleF rect, float cornerRadius, UIColor[] colors, float[] colorLocations, float strokeLineWidth, 
                              UIColor strokeLineColor, ShadowSetting innerShadow, ShadowSetting dropShadow, 
                              string title, UIColor titleColor, UIFont titleFont, bool useShadow = true,  string image = null) : base ( rect )
        {
            if ( useShadow )
            {
                _textShadowColor = UIColor.FromRGBA(0f, 0f, 0f, 0.5f);
            }
            _colors = colors;
            _colorLocations = colorLocations;
            _strokeLineColor = strokeLineColor;
            _strokeLineWidth = strokeLineWidth;
            _cornerRadius = cornerRadius;
            _title = title;
            _titleColor = titleColor.CGColor;
            _titleFont = titleFont;
            _innerShadow = innerShadow;
            _dropShadow = dropShadow;
            BackgroundColor = UIColor.Clear;
            Layer.MasksToBounds = false;
            ClipsToBounds = false;
            if (image != null)
            {
                _image = UIImage.FromFile(image);
            }
            SetTitleColor( UIColor.Clear, UIControlState.Normal );
            SetTitleColor( UIColor.Clear, UIControlState.Highlighted);
            SetTitleColor( UIColor.Clear, UIControlState.Selected );
        }


        public ShadowSetting DropShadow
        {
            get{ return _dropShadow; }
            set
            {
                _dropShadow = value;
                SetNeedsDisplay();
            }
        }

        public ShadowSetting InnerShadow
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

        public UIColor[] Colors
        {
            get{ return _colors; }
            set
            {
                _colors = value;
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
            Console.WriteLine ( "Cc" );
            var touch = uievent.AllTouches.AnyObject as UITouch;
            if (Bounds.Contains (touch.LocationInView (this)))
                _pressed = true;
            else
                _pressed = false;

            SetNeedsDisplay();
            return base.ContinueTracking (uitouch, uievent);
        }

        public override void SetTitle(string title, UIControlState forState)
        {
            SetNeedsDisplay();
            _title = title;
        }

        public override void Draw(RectangleF rect)
        {
            base.Draw(rect);

            var colorSpace = CGColorSpace.CreateDeviceRGB();
            var context = UIGraphics.GetCurrentContext();

            var newGradientColors = _colors.Select( c=>c.CGColor).ToArray() ;
            var newGradientLocations = _colorLocations;
            var newGradient = new CGGradient(colorSpace, newGradientColors, newGradientLocations);

            rect.Width -= _dropShadow != null ? Math.Abs(_dropShadow.Offset.Width) : 0;
            rect.Height -= _dropShadow != null ? Math.Abs(_dropShadow.Offset.Height) : 0;
            rect.X += _dropShadow != null && _dropShadow.Offset.Width < 0 ? Math.Abs(_dropShadow.Offset.Width) : 0;
            rect.Y += _dropShadow != null && _dropShadow.Offset.Height < 0 ? Math.Abs(_dropShadow.Offset.Height) : 0;

            var roundedRectanglePath = UIBezierPath.FromRoundedRect(rect, _cornerRadius);
            context.SaveState();
            if (_dropShadow != null)
            {
                context.SetShadowWithColor(_dropShadow.Offset, _dropShadow.BlurRadius, _dropShadow.Color.CGColor);
            }

            context.BeginTransparencyLayer(null);
            roundedRectanglePath.AddClip();
            context.DrawLinearGradient(newGradient, new PointF(rect.X + (rect.Width / 2.0f), rect.Y), new PointF(rect.X + (rect.Width / 2.0f), rect.Y + rect.Height), 0);
            context.EndTransparencyLayer();
            context.RestoreState();

            if (_innerShadow != null)
            {
                var roundedRectangleBorderRect = roundedRectanglePath.Bounds;
                roundedRectangleBorderRect.Inflate(_innerShadow.BlurRadius, _innerShadow.BlurRadius);
                roundedRectangleBorderRect.Offset(-_innerShadow.Offset.Width, -_innerShadow.Offset.Height);
                roundedRectangleBorderRect = RectangleF.Union(roundedRectangleBorderRect, roundedRectanglePath.Bounds);
                roundedRectangleBorderRect.Inflate(1, 1);

                var roundedRectangleNegativePath = UIBezierPath.FromRect(roundedRectangleBorderRect);
                roundedRectangleNegativePath.AppendPath(roundedRectanglePath);
                roundedRectangleNegativePath.UsesEvenOddFillRule = true;

                context.SaveState();
                {
                    var xOffset = _innerShadow.Offset.Width + (float)Math.Round(roundedRectangleBorderRect.Width);
                    var yOffset = _innerShadow.Offset.Height;
                    context.SetShadowWithColor(
                        new SizeF(xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)),
                        _innerShadow.BlurRadius,
                        _innerShadow.Color.CGColor);

                    roundedRectanglePath.AddClip();
                    var transform = CGAffineTransform.MakeTranslation(-(float)Math.Round(roundedRectangleBorderRect.Width), 0);
                    roundedRectangleNegativePath.ApplyTransform(transform);
                    UIColor.Gray.SetFill();
                    roundedRectangleNegativePath.Fill();
                }
                context.RestoreState();
            }

            context.SaveState();
            roundedRectanglePath.LineWidth = _strokeLineWidth;
            _strokeLineColor.SetStroke();
            roundedRectanglePath.AddClip();
            context.AddPath(roundedRectanglePath.CGPath);
            context.StrokePath();
            context.RestoreState();

            RectangleF imageRect = new RectangleF();
            if (_image != null)
            {
                imageRect = new RectangleF(3f, 3f, rect.Height - 6f, rect.Height - 6f);
                _image.Draw(imageRect, CGBlendMode.Normal, 1f);
            }

            context.SaveState();
            context.SelectFont(_titleFont.Name, _titleFont.PointSize, CGTextEncoding.MacRoman);
            context.SetTextDrawingMode(CGTextDrawingMode.Fill);
            context.SetStrokeColor(_titleColor);
            if (_textShadowColor != null)
            {
                context.SetShadowWithColor(new SizeF(0f, -0.5f), 0.5f, _textShadowColor.CGColor);
            }
            context.SetFillColor(_titleColor);
            context.TextMatrix = new CGAffineTransform(1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f);
            var titleSize = ((NSString)_title).StringSize(_titleFont);

            context.ShowTextAtPoint(((rect.Width - imageRect.Width) / 2) - (titleSize.Width / 2) + imageRect.Right, rect.GetMidY() + (_titleFont.PointSize / 3), _title);
            context.RestoreState();

            if (_pressed)
            {
                var insideRect = rect.Inset(0f, 0f);
                var container = UIBezierPath.FromRoundedRect(insideRect, _cornerRadius);
                context.SaveState();
                container.AddClip();

                _highlightedColor.SetFill();
                context.FillRect(insideRect);
                context.RestoreState();
            }
        }
    }
}

