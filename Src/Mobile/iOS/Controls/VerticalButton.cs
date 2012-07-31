using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class VerticalButton : UIButton
	{
		private UIColor _background;
		private UIColor _customSelectedColor;
		private CGColor _gradientStart;
		private CGColor _gradientEnd;
		private CGColor _customSelectedGradientStart;
		private CGColor _customSelectedGradientEnd;

		public VerticalButton ( RectangleF rect, UIColor background ) : base( rect )
		{
			_background = background;
		}

		public VerticalButton ( RectangleF rect, UIColor gradientStart, UIColor gradientEnd ) : base( rect )
		{
			_gradientStart = gradientStart.CGColor;
			_gradientEnd = gradientEnd.CGColor;
		}

		public void SetCustomSelectedBackgroundColor( UIColor startColor, UIColor endColor )
		{
			_customSelectedGradientStart = startColor.CGColor;
			_customSelectedGradientEnd = endColor.CGColor;
		}

		public void SetCustomSelectedBackgroundColor( UIColor solidColor )
		{
			_customSelectedColor = solidColor;
		}

		public bool FirstButton { get; set; }
		public bool LastButton { get; set; }
		public bool IsGradient { get { return (_customSelectedGradientStart != null && _customSelectedGradientEnd != null && Selected ) || (_gradientStart != null && _gradientEnd != null && !Selected ); } }

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);

			var colorSpace = CGColorSpace.CreateDeviceRGB();
			var context = UIGraphics.GetCurrentContext();

			List<UIBezierPath> _exteriorPaths = new List<UIBezierPath>();
			List<UIBezierPath> _interiorBottomPaths = new List<UIBezierPath>();
			List<UIBezierPath> _interiorTopPaths = new List<UIBezierPath>();

			UIBezierPath rectanglePath;
			if( FirstButton )
			{
				if( Selected )
				{
					rectanglePath = UIBezierPath.FromRoundedRect(rect, UIRectCorner.TopLeft | UIRectCorner.TopRight, new SizeF(4, 4));
				}
				else
				{
					rectanglePath = UIBezierPath.FromRoundedRect(rect, 4);
				}

				_exteriorPaths.Add( rectanglePath );
			}
			else if( LastButton )
			{
				rectanglePath = UIBezierPath.FromRoundedRect(rect, UIRectCorner.BottomLeft | UIRectCorner.BottomRight, new SizeF(4, 4));

				var path = new UIBezierPath();
				path.MoveTo( new PointF( rect.X + 1, rect.Y ) );
				path.AddLineTo( new PointF( rect.Right -1, rect.Y ) );
				_interiorTopPaths.Add( path ); 

				path = new UIBezierPath();
				path.MoveTo( new PointF( rect.X, rect.Y ) );
				path.AddLineTo( new PointF( rect.X, rect.Bottom - 4) );
				path.AddArc( new PointF( rect.X + 4, rect.Bottom - 4), 4f, ((float)Math.PI),((float)(Math.PI/2)), false );
				path.AddLineTo( new PointF( rect.Right - 4, rect.Bottom ) );
				path.AddArc( new PointF( rect.Right - 4, rect.Bottom - 4 ), 4f, ((float)(Math.PI/2)), ((float)(2*Math.PI)), false );
				path.AddLineTo( new PointF( rect.Right, rect.Y ) );
				_exteriorPaths.Add( path );

			}
			else
			{
				rectanglePath = UIBezierPath.FromRect( rect );

				var path = new UIBezierPath();
				path.MoveTo( new PointF( rect.X + 1, rect.Bottom ) );
				path.AddLineTo( new PointF( rect.Right - 1, rect.Bottom ) );
				_interiorBottomPaths.Add( path ); 
				                         
				path = new UIBezierPath();
				path.MoveTo( new PointF( rect.X + 1, rect.Y ) );
				path.AddLineTo( new PointF( rect.Right -1, rect.Y ) );
				_interiorTopPaths.Add( path ); 

				path = new UIBezierPath();
				path.MoveTo( new PointF( rect.X, rect.Y ) );
				path.AddLineTo( new PointF( rect.X, rect.Bottom ) );
				_exteriorPaths.Add( path ); 
			
				path = new UIBezierPath();
				path.MoveTo( new PointF( rect.Right, rect.Y ) );
				path.AddLineTo( new PointF( rect.Right, rect.Bottom ) );
				_exteriorPaths.Add( path ); 
			}

			if( IsGradient )
			{
				CGColor[] newGradientColors;
				if( Selected )
				{
					newGradientColors = new CGColor [] {_customSelectedGradientStart, _customSelectedGradientEnd};
				}
				else
				{
					newGradientColors = new CGColor [] {_gradientStart, _gradientEnd};
				}

				var newGradientLocations = new float [] {0, 1};
				var newGradient = new CGGradient(colorSpace, newGradientColors, newGradientLocations);

				context.SaveState();
				rectanglePath.AddClip();
				context.DrawLinearGradient(newGradient, new PointF(( rect.Width / 2 ) + rect.X, rect.Y), new PointF((rect.Width/ 2 ) + rect.X, rect.Y + rect.Height), 0);
				context.RestoreState();
			}
			else
			{
				if( !Selected )
				{
					_background.SetFill();
				}
				else
				{
					_customSelectedColor.SetFill();
				}
				rectanglePath.Fill();
			}


			//Inner shadow for the top button
			if( FirstButton )
			{
				//// Shadow Declarations
				var shadow2 = UIColor.White.ColorWithAlpha(0.5f).CGColor;
				var shadow2Offset = new SizeF(0, 1);
				var shadow2BlurRadius = 0;
				////// Rounded Rectangle Inner Shadow
				var roundedRectangleBorderRect = new RectangleF( rect.X, rect.Y+1, rect.Width, rect.Height - 1 ); // rectanglePath.Bounds;
				roundedRectangleBorderRect.Inflate(shadow2BlurRadius, shadow2BlurRadius);
				roundedRectangleBorderRect.Offset(-shadow2Offset.Width, -shadow2Offset.Height);
				roundedRectangleBorderRect = RectangleF.Union(roundedRectangleBorderRect, new RectangleF( rect.X, rect.Y+1, rect.Width, rect.Height - 1 ));
				roundedRectangleBorderRect.Inflate(1, 1);

				var roundedRectangleNegativePath = UIBezierPath.FromRect(roundedRectangleBorderRect);
				var rectanglePath2 = UIBezierPath.FromRoundedRect(new RectangleF( rect.X, rect.Y+1, rect.Width, rect.Height - 1 ), UIRectCorner.TopLeft | UIRectCorner.TopRight, new SizeF(4, 4));
				roundedRectangleNegativePath.AppendPath(rectanglePath2);
				roundedRectangleNegativePath.UsesEvenOddFillRule = true;

				context.SaveState();
				{
				    var xOffset = shadow2Offset.Width + (float)Math.Round(roundedRectangleBorderRect.Width);
				    var yOffset = shadow2Offset.Height;
				    context.SetShadowWithColor(
				        new SizeF(xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)),
				        shadow2BlurRadius,
				        shadow2);

				    rectanglePath2.AddClip();
				    var transform = CGAffineTransform.MakeTranslation(-(float)Math.Round(roundedRectangleBorderRect.Width), 0);
				    roundedRectangleNegativePath.ApplyTransform(transform);
				    UIColor.Gray.SetFill();
				    roundedRectangleNegativePath.Fill();
				}
				context.RestoreState();
			}

			//Strokes
			if( FirstButton && !Selected )
			{
				UIColor.FromRGB(127,127,127).SetStroke();
			}
			else
			{
				UIColor.FromRGB(36,44,51).SetStroke();
			}
			_exteriorPaths.ForEach( p => {
				p.LineWidth = 1;
				p.Stroke();
			});

			UIColor.FromRGB(253,253,253).SetStroke();
			_interiorTopPaths.ForEach( p => {
				p.LineWidth = 1;
				p.Stroke();
			});

			UIColor.FromRGB(207,211,214).SetStroke();
			_interiorBottomPaths.ForEach( p => {
				p.LineWidth = 1;
				p.Stroke();
			});
		}
	}
}

