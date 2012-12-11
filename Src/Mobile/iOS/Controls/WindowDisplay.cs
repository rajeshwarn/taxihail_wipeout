//using System;
//using System.Linq;
//using MonoTouch.UIKit;
//using System.Drawing;
//using MonoTouch.CoreGraphics;
//using MonoTouch.Foundation;
//
//
//namespace apcurium.MK.Booking.Mobile.Client
//{
//    [Register ("WindowDisplay")]
//    public class WindowDisplay : UIView
//    {
//        private UIColor[] _colors = new UIColor[]
//            {
//                UIColor.FromRGBA(255, 255, 255, 0.03f ),
//                UIColor.FromRGBA(0, 0, 0, 0.03f )
//            };
//        private float[] _colorLocations = new float[] { 1f, 0f };
//        private float _strokeLineWidth = 1f;
//        private UIColor _strokeLineColor = UIColor.FromRGB(105, 105, 105) ;
//        private float _cornerRadius = AppStyle.ButtonCornerRadius;
//		private Style.ShadowDefinition _innerShadow = new Style.ShadowDefinition(){ BlurRadius = 2f, Color = new Style.ColorDefinition() { Red = 0, Green = 0, Blue = 0, Alpha = 128 }, OffsetX = 0, OffsetY = 1 };
//        private Style.ShadowDefinition _dropShadow = new Style.ShadowDefinition(){ BlurRadius = 0f, Color = new Style.ColorDefinition() { Red = 255, Green = 255, Blue = 255, Alpha = 77 }, OffsetX = 0, OffsetY = 1 };
//
//        public WindowDisplay(IntPtr handle) : base(  handle )
//        {
//            Layer.MasksToBounds = false;
//            ClipsToBounds = false;
//            BackgroundColor = UIColor.Clear;
//        }
//
//        public override void Draw(RectangleF rect)
//        {
//            base.Draw(rect);
//
////            var colorSpace = CGColorSpace.CreateDeviceRGB();
////            var context = UIGraphics.GetCurrentContext();
////
////            var newGradientColors = _colors.Select( c=>c.CGColor).ToArray() ;
////            var newGradientLocations = _colorLocations;
////            var newGradient = new CGGradient(colorSpace, newGradientColors, newGradientLocations);
////
////            rect.Width -= _dropShadow != null ? Math.Abs(_dropShadow.OffsetX) : 0;
////            rect.Height -= _dropShadow != null ? Math.Abs(_dropShadow.OffsetY) : 0;
////            rect.X += _dropShadow != null && _dropShadow.OffsetX < 0 ? Math.Abs(_dropShadow.OffsetX) : 0;
////            rect.Y += _dropShadow != null && _dropShadow.OffsetY < 0 ? Math.Abs(_dropShadow.OffsetY) : 0;
////
////			context.SaveState();
////            var roundedRectanglePath = UIBezierPath.FromRoundedRect(rect, _cornerRadius);
////            if (_dropShadow != null)
////            {
////                context.SetShadowWithColor( new SizeF( _dropShadow.OffsetX, _dropShadow.OffsetY ), _dropShadow.BlurRadius, UIColor.FromRGBA( _dropShadow.Color.Red, _dropShadow.Color.Green, _dropShadow.Color.Blue, _dropShadow.Color.Alpha).CGColor);
////            }
////			UIColor.White.SetFill();
////			roundedRectanglePath.Fill();
////			context.RestoreState();
////            
////			context.SaveState();
////            context.BeginTransparencyLayer(null);
////            roundedRectanglePath.AddClip();
////            context.DrawLinearGradient(newGradient, new PointF(rect.X + (rect.Width / 2.0f), rect.Y), new PointF(rect.X + (rect.Width / 2.0f), rect.Y + rect.Height), 0);
////            context.EndTransparencyLayer();
////            context.RestoreState();
////
////            if (_innerShadow != null)
////            {
////                var roundedRectangleBorderRect = roundedRectanglePath.Bounds;
////                roundedRectangleBorderRect.Inflate(_innerShadow.BlurRadius, _innerShadow.BlurRadius);
////                roundedRectangleBorderRect.Offset(-_innerShadow.OffsetX, -_innerShadow.OffsetY);
////                roundedRectangleBorderRect = RectangleF.Union(roundedRectangleBorderRect, roundedRectanglePath.Bounds);
////                roundedRectangleBorderRect.Inflate(1, 1);
////
////                var roundedRectangleNegativePath = UIBezierPath.FromRect(roundedRectangleBorderRect);
////                roundedRectangleNegativePath.AppendPath(roundedRectanglePath);
////                roundedRectangleNegativePath.UsesEvenOddFillRule = true;
////
////                context.SaveState();
////                {
////                    var xOffset = _innerShadow.OffsetX + (float)Math.Round(roundedRectangleBorderRect.Width);
////                    var yOffset = _innerShadow.OffsetY;
////                    context.SetShadowWithColor(
////                        new SizeF(xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)),
////                        _innerShadow.BlurRadius,
////                        UIColor.FromRGBA( _innerShadow.Color.Red, _innerShadow.Color.Green, _innerShadow.Color.Blue, _innerShadow.Color.Alpha).CGColor);
////
////                    roundedRectanglePath.AddClip();
////                    var transform = CGAffineTransform.MakeTranslation(-(float)Math.Round(roundedRectangleBorderRect.Width), 0);
////                    roundedRectangleNegativePath.ApplyTransform(transform);
////                    UIColor.Gray.SetFill();
////                    roundedRectangleNegativePath.Fill();
////                }
////                context.RestoreState();
////            }
////
////            context.SaveState();
////            roundedRectanglePath.LineWidth = _strokeLineWidth;
////            _strokeLineColor.SetStroke();
////            roundedRectanglePath.AddClip();
////            context.AddPath(roundedRectanglePath.CGPath);
////            context.StrokePath();
////            context.RestoreState();
//
//        }
//    }
//}
//
