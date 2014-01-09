using System;
using System.Drawing;
using System.Linq;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Style;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public sealed class SegmentedButton : UIButton
    {
		public bool Pressed {get;set;}
        private readonly UIColor[] _normalColors =
        {
            UIColor.FromRGB(71, 71, 71),
            UIColor.FromRGB(0, 0, 0),
            UIColor.FromRGB(0, 0, 0)
        };
        private readonly UIColor[] _highlightedColors =
        {
            UIColor.FromRGB(39, 39, 39),
            UIColor.FromRGB(100, 100, 100)
        };
		private readonly float[] _normalColorLocations = { 0f, 0.83f, 1f };
        private readonly float[] _highlightedColorLocations = { 0f, 1f };
        private float _cornerRadius = 0;
        
       	
		private readonly UIColor _normalTitleColor = UIColor.FromRGB(163, 163, 163);
		private readonly UIColor _highlightedTitleColor = UIColor.White;
		private readonly UIFont _titleFont = AppStyle.ButtonFont;
		private readonly UIColor _normalTextShadowColor = UIColor.FromRGBA ( 0, 0, 0, 255 );
        private readonly UIColor _highlightedTextShadowColor = UIColor.FromRGBA ( 0, 0, 0, 128 );

		private readonly ShadowDefinition _highlightedInnerShadow = new ShadowDefinition{ Color = new ColorDefinition{ Red = 0, Green = 0, Blue=0, Alpha=192}, OffsetX=0, OffsetY=0, BlurRadius=7};
        private readonly ShadowDefinition _normalInnerShadow = new ShadowDefinition{ Color = new ColorDefinition{ Red = 0, Green = 0, Blue=0, Alpha=255}, OffsetX=1, OffsetY=0, BlurRadius=0};
		private readonly ShadowDefinition _dropShadow = new ShadowDefinition{ Color = new ColorDefinition{ Red = 72, Green = 72, Blue=72, Alpha=255}, OffsetX=1, OffsetY=0, BlurRadius=0};

		private readonly string _title;

		public SegmentedButton( RectangleF rect, string title ) : base ( rect )
        {
			_title = title;
          	BackgroundColor = UIColor.Clear;
            Layer.MasksToBounds = false;
            ClipsToBounds = false;

			TouchUpInside += delegate
			{
				Selected = !Selected;
				if (Selected
				    && SelectedChangedCommand != null
				    && SelectedChangedCommand.CanExecute())
				{
					SelectedChangedCommand.Execute(Tag2);
				}
			};
        }

// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public ICommand SelectedChangedCommand { get; set; }

		public string Tag2 { get; set; }

        public override void Draw(RectangleF rect)
        {
            base.Draw(rect);

            var colorSpace = CGColorSpace.CreateDeviceRGB();
            var context = UIGraphics.GetCurrentContext();

			CGColor[] newGradientColors;
			float[] newGradientLocations;
			if( !Pressed )
			{
	            newGradientColors = _normalColors.Select( c=>c.CGColor).ToArray() ;
	            newGradientLocations = _normalColorLocations;
			}
			else
			{
	            newGradientColors = _highlightedColors.Select( c=>c.CGColor).ToArray() ;
	            newGradientLocations = _highlightedColorLocations;
			}

			var newGradient = new CGGradient(colorSpace, newGradientColors, newGradientLocations);

            rect.Width -= !Pressed ? Math.Abs(_dropShadow.OffsetX) : 0;
            rect.Height -= !Pressed ? Math.Abs(_dropShadow.OffsetY) : 0;
            rect.X += !Pressed && _dropShadow.OffsetX < 0 ? Math.Abs(_dropShadow.OffsetX) : 0;
            rect.Y += !Pressed && _dropShadow.OffsetY < 0 ? Math.Abs(_dropShadow.OffsetY) : 0;

            var roundedRectanglePath = UIBezierPath.FromRoundedRect(rect, _cornerRadius);
			UIColor.Black.SetFill();
			roundedRectanglePath.Fill();

            context.SaveState();
            if ( !Pressed )
            {
                context.SetShadowWithColor( new SizeF( _dropShadow.OffsetX, _dropShadow.OffsetY ), _dropShadow.BlurRadius, UIColor.FromRGBA( _dropShadow.Color.Red, _dropShadow.Color.Green, _dropShadow.Color.Blue, _dropShadow.Color.Alpha ).CGColor );
            }

            context.BeginTransparencyLayer(null);
            roundedRectanglePath.AddClip();
            context.DrawLinearGradient(newGradient, new PointF(rect.X + (rect.Width / 2.0f), rect.Y), new PointF(rect.X + (rect.Width / 2.0f), rect.Y + rect.Height), 0);
            context.EndTransparencyLayer();
            context.RestoreState();
            
			var innerShadow = Pressed ? _highlightedInnerShadow : _normalInnerShadow;
		
            var roundedRectangleBorderRect = roundedRectanglePath.Bounds;
            roundedRectangleBorderRect.Inflate(innerShadow.BlurRadius, innerShadow.BlurRadius);
            roundedRectangleBorderRect.Offset(-innerShadow.OffsetX, -innerShadow.OffsetY);
            roundedRectangleBorderRect = RectangleF.Union(roundedRectangleBorderRect, roundedRectanglePath.Bounds);
            roundedRectangleBorderRect.Inflate(1, 1);

            var roundedRectangleNegativePath = UIBezierPath.FromRect(roundedRectangleBorderRect);
            roundedRectangleNegativePath.AppendPath(roundedRectanglePath);
            roundedRectangleNegativePath.UsesEvenOddFillRule = true;

            context.SaveState();
            {
				context.SetBlendMode( Pressed ? CGBlendMode.Normal : CGBlendMode.Multiply );
                var xOffset = innerShadow.OffsetX + (float)Math.Round(roundedRectangleBorderRect.Width);
                var yOffset = innerShadow.OffsetY;
                context.SetShadowWithColor(
                    new SizeF(xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)),
                    innerShadow.BlurRadius,
                    UIColor.FromRGBA( innerShadow.Color.Red, innerShadow.Color.Green, innerShadow.Color.Blue, innerShadow.Color.Alpha ).CGColor);

                roundedRectanglePath.AddClip();
                var transform = CGAffineTransform.MakeTranslation(-(float)Math.Round(roundedRectangleBorderRect.Width), 0);
                roundedRectangleNegativePath.ApplyTransform(transform);
                UIColor.Gray.SetFill();
                roundedRectangleNegativePath.Fill();
            }
            context.RestoreState();

            context.SaveState();
            context.SetFont(CGFont.CreateWithFontName(_titleFont.Name));
            context.SetFontSize(_titleFont.PointSize);
            context.SetTextDrawingMode(CGTextDrawingMode.Fill);
            context.SetStrokeColor(Pressed ? _highlightedTitleColor.CGColor : _normalTitleColor.CGColor);
            context.SetShadowWithColor(new SizeF(0f, -0.5f), 0.5f, Pressed ? _highlightedTextShadowColor.CGColor : _normalTextShadowColor.CGColor);
            context.SetFillColor( Pressed ? _highlightedTitleColor.CGColor : _normalTitleColor.CGColor);
            context.TextMatrix = new CGAffineTransform(1.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f);
            var stringAttributes = new UIStringAttributes
            {
                Font = _titleFont,
                ParagraphStyle = new NSParagraphStyle(),
            };
            stringAttributes.ParagraphStyle.Alignment = UITextAlignment.Center;

            var rectText = new RectangleF(rect.X, rect.GetMidY() + (_titleFont.PointSize / 3), rect.Width, rect.Height);
            ((NSString)_title).DrawString(rectText, stringAttributes);
            context.RestoreState();

        }
    }
}

