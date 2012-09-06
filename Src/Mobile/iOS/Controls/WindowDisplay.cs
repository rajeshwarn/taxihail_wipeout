using System;
using System.Linq;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;


namespace apcurium.MK.Booking.Mobile.Client
{
    [Register ("WindowDisplay")]
    public class WindowDisplay : UIView
    {
        private UIColor[] _colors = new UIColor[]
            {
                UIColor.FromRGBA(255, 255, 255, 0.03f ),
                UIColor.FromRGBA(0, 0, 0, 0.03f )
            };
        private float[] _colorLocations = new float[] { 1f, 0f };
        private float _strokeLineWidth = 1f;
        private UIColor _strokeLineColor = UIColor.FromRGB(105, 105, 105) ;
        private float _cornerRadius = AppStyle.ButtonCornerRadius;
		private ShadowSetting _innerShadow = new ShadowSetting(){ BlurRadius = 3f, Color = UIColor.Black, Offset = new SizeF(0, 1) };
        private ShadowSetting _dropShadow = new ShadowSetting(){ BlurRadius = 0f, Color = UIColor.White.ColorWithAlpha( 0.5f ), Offset = new SizeF(0, 1) };

        public WindowDisplay(IntPtr handle) : base(  handle )
        {

            BackgroundColor = UIColor.White;
            Layer.MasksToBounds = false;
            ClipsToBounds = false;
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

        }
    }
}

