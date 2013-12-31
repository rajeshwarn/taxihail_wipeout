using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
	public sealed class CustomCellBackgroundView : UIView
	{
		private bool _isTop;
		private bool _isBottom;
	    private const float CornerRadius = 3f;
	    private const float StrokeSize = 1f;
	    private const float InnerShadowTopBottomBlurRadius = 3f;
	    private const float InnerShadowSidesBlurRadius = 2f;
	    private readonly UIColor _strokeColor = UIColor.FromRGB( 133, 133, 133 );
		private readonly UIColor _selectedBackgroundColor = UIColor.FromRGBA( 233, 217, 219, 0.1f );
		private readonly UIColor _backgroundColor = AppStyle.CellBackgroundColor;

		public CustomCellBackgroundView(bool isTop, bool isBottom, RectangleF rect, bool isAddNewCell ) : base( rect )
		{
			_isTop = isTop;
			_isBottom = isBottom;
			BackgroundColor = UIColor.Clear;
			IsAddNewCell = isAddNewCell;

		}

// ReSharper disable once UnusedAutoPropertyAccessor.Global
		public bool IsAddNewCell { get; set; }

		public bool IsTop { 
			get { return _isTop; } 
			set { _isTop = value; }
		}

		public bool IsBottom { 
			get { return _isBottom; } 
			set { _isBottom = value; }
		}

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);

			var context = UIGraphics.GetCurrentContext();

			context.SetLineWidth( StrokeSize );

			UIBezierPath fillRectPath;
			var strokePath = new UIBezierPath();
			if( _isTop && _isBottom )
			{
				fillRectPath = UIBezierPath.FromRoundedRect( rect, CornerRadius );
				strokePath = fillRectPath;
			}
			else if( _isTop )
			{
				fillRectPath = UIBezierPath.FromRoundedRect(rect, UIRectCorner.TopLeft | UIRectCorner.TopRight, new SizeF(CornerRadius,CornerRadius) );
				strokePath.MoveTo( new PointF( rect.Left, rect.Bottom ));
				strokePath.AddLineTo( new PointF( rect.Left, rect.Top + CornerRadius ));
				strokePath.AddArc( new PointF( rect.Left + CornerRadius, rect.Top + CornerRadius), CornerRadius, (float)Math.PI, (float)(3*Math.PI/2), true );
				strokePath.AddLineTo( new PointF( rect.Right - CornerRadius, rect.Top ));
				strokePath.AddArc( new PointF( rect.Right - CornerRadius, rect.Top + CornerRadius), CornerRadius, (float)(3*Math.PI/2), 0, true );
				strokePath.AddLineTo( new PointF( rect.Right, rect.Bottom ));
				strokePath.AddLineTo( new PointF( rect.Left, rect.Bottom ));
			}
			else if( _isBottom )
			{
				fillRectPath = UIBezierPath.FromRoundedRect(rect, UIRectCorner.BottomLeft | UIRectCorner.BottomRight, new SizeF(CornerRadius,CornerRadius) );
				strokePath.MoveTo( new PointF( rect.Right, rect.Top ));
				strokePath.AddLineTo( new PointF( rect.Right, rect.Bottom - CornerRadius ));
				strokePath.AddArc( new PointF( rect.Right - CornerRadius, rect.Bottom - CornerRadius), CornerRadius, 0, (float)(Math.PI/2), true );
				strokePath.AddLineTo( new PointF( rect.Left + CornerRadius, rect.Bottom ));
				strokePath.AddArc( new PointF( rect.Left + CornerRadius, rect.Bottom - CornerRadius), CornerRadius, (float)(Math.PI/2), (float)Math.PI, true );
				strokePath.AddLineTo( new PointF( rect.Left, rect.Top) );
			}
			else
			{
				fillRectPath = UIBezierPath.FromRect( rect );
				strokePath.MoveTo( new PointF( rect.Left, rect.Top) );
				strokePath.AddLineTo( new PointF( rect.Left, rect.Bottom ));
				strokePath.AddLineTo( new PointF( rect.Right, rect.Bottom) );
				strokePath.AddLineTo( new PointF( rect.Right, rect.Top) );
			}

			//Fill
            var backgroundColor = UIColor.Clear;
            if (Superview is UITableViewCell)
            {
                backgroundColor = ((UITableViewCell)Superview).Highlighted ? _selectedBackgroundColor : _backgroundColor;
            }
            else if (Superview.Superview is UITableViewCell)
            {
                backgroundColor = ((UITableViewCell)Superview.Superview).Highlighted ? _selectedBackgroundColor : _backgroundColor;
            }
			backgroundColor.SetFill();
			fillRectPath.LineWidth = StrokeSize;
			fillRectPath.Fill();

			//Inner Shadow
		    var roundedRectangleBorderRect = fillRectPath.Bounds;
			roundedRectangleBorderRect.X -= InnerShadowSidesBlurRadius;
			roundedRectangleBorderRect.Width += 2*InnerShadowSidesBlurRadius;
			roundedRectangleBorderRect.Y -= _isTop ? InnerShadowTopBottomBlurRadius : 0;
			roundedRectangleBorderRect.Height += _isBottom ? 2*InnerShadowTopBottomBlurRadius : 0;

            roundedRectangleBorderRect = RectangleF.Union(roundedRectangleBorderRect, fillRectPath.Bounds);
			roundedRectangleBorderRect.X -= 1;
			roundedRectangleBorderRect.Width += 2;
			roundedRectangleBorderRect.Y -= 0;
			roundedRectangleBorderRect.Height += 0;

            var roundedRectangleNegativePath = UIBezierPath.FromRect(roundedRectangleBorderRect);
            roundedRectangleNegativePath.AppendPath(fillRectPath);
            roundedRectangleNegativePath.UsesEvenOddFillRule = true;

            context.SaveState();
            {
                var xOffset = (float)Math.Round(roundedRectangleBorderRect.Width);
				var yOffset = 0;
                context.SetShadowWithColor(
                    new SizeF(xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)),
                    InnerShadowTopBottomBlurRadius,
					_strokeColor.CGColor);

                fillRectPath.AddClip();
                var transform = CGAffineTransform.MakeTranslation(-(float)Math.Round(roundedRectangleBorderRect.Width), 0);
                roundedRectangleNegativePath.ApplyTransform(transform);
                UIColor.Gray.SetFill();
                roundedRectangleNegativePath.Fill();
            }
            context.RestoreState();

			//Stroke
			context.SaveState();
            strokePath.LineWidth = StrokeSize;
            _strokeColor.SetStroke();
            strokePath.AddClip();
            context.AddPath(strokePath.CGPath);
            context.StrokePath();
            context.RestoreState();     

		}
	}
}

