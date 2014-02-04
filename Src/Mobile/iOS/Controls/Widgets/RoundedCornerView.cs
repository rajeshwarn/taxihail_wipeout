using System;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	public class RoundedCornerView : UIView
	{
		private UIRectCorner _roundedCorners = UIRectCorner.AllCorners;
		private float _strokeLineWidth = UIHelper.OnePixel;
		private UIColor _strokeLineColor = UIColor.FromRGB(155, 155, 155) ;
		private float _cornerRadius = 3;
		public bool FirstRowOfTwoRowsTable = false;
		private UIColor _backgroundColor;


		public RoundedCornerView()
		{
			ClipsToBounds = true;
			Layer.ShouldRasterize = true;
			_backgroundColor = UIColor.Red;
			BackgroundColor = UIColor.Clear;
			Borders = Border.All;
		}

		//only use when there's no corner
		public Border Borders
		{
			get; set;
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

		public UIColor BackColor
		{
			get{ return _backgroundColor; }
			set
			{
				_backgroundColor = value;
				SetNeedsDisplay();
			}
		}

		public UIRectCorner Corners
		{
			get{ return _roundedCorners; }
			set
			{
				_roundedCorners = value;
				SetNeedsDisplay();
			}
		}

		public override void Draw(System.Drawing.RectangleF rect)
		{
			base.Draw(rect);

			var context = UIGraphics.GetCurrentContext ();
			if (Corners != 0)
			{
				var roundedRectanglePath = UIBezierPath.FromRoundedRect (rect, _roundedCorners, new SizeF (_cornerRadius, _cornerRadius));
				DrawBackground (rect, context, _backgroundColor.CGColor, roundedRectanglePath);
				DrawStroke(context, roundedRectanglePath);
			}
			else
			{
				DrawBackground (rect, context, _backgroundColor.CGColor, null);
				DrawStrokePartial(context, rect);
			}
		}


		void DrawBackground (RectangleF rect, CGContext context, CGColor color, UIBezierPath roundedRectanglePath)
		{
			context.SaveState ();
			context.BeginTransparencyLayer (null);
			if (roundedRectanglePath != null)
			{
				roundedRectanglePath.AddClip ();
			}
			context.SetFillColorWithColor(color);
			context.FillRect(rect);
			context.EndTransparencyLayer ();
			context.RestoreState ();
		}

		void DrawStrokePartial(CGContext context, RectangleF rect)
		{
			context.SaveState ();

			var bezierPath = new UIBezierPath();

			bezierPath.MoveTo(new PointF(0, 0));

			if((Borders & Border.Left) == Border.Left)
			{
				bezierPath.AddLineTo(new PointF(0,rect.Height));
			}
			bezierPath.MoveTo(new PointF(0, rect.Height));

			if((Borders & Border.Bottom) == Border.Bottom)
			{
				bezierPath.AddLineTo(new PointF(rect.Width,rect.Height));
			}
			bezierPath.MoveTo(new PointF(rect.Width,rect.Height));

			if((Borders & Border.Right) == Border.Right)
			{
				bezierPath.AddLineTo(new PointF(rect.Width,0));
			}
			bezierPath.MoveTo(new PointF(rect.Width,0));

			if((Borders & Border.Top) == Border.Top)
			{
				bezierPath.AddLineTo(new PointF(0,0));
			}
			bezierPath.MoveTo(new PointF(0,0));


			bezierPath.LineWidth = _strokeLineWidth;
			var strokeLineColor = _strokeLineColor;
			strokeLineColor.SetStroke ();
			context.AddPath(bezierPath.CGPath);
			context.StrokePath();
			context.RestoreState ();
		}

		protected virtual void DrawStroke (CGContext context, UIBezierPath roundedRectanglePath)
		{
			context.SaveState ();
			roundedRectanglePath.LineWidth = _strokeLineWidth;
			var strokeLineColor = _strokeLineColor;
			strokeLineColor.SetStroke ();
			roundedRectanglePath.AddClip();
			context.AddPath(roundedRectanglePath.CGPath);
			context.StrokePath();
			context.RestoreState ();

			//we remove the bottom line drawn by the roundedRect bezier path to remove the doubled line effect
			if (FirstRowOfTwoRowsTable)
			{
				context.SaveState ();
				roundedRectanglePath.LineWidth = _strokeLineWidth;
				UIColor.White.SetStroke ();
				context.MoveTo(0, 40);
				context.AddLineToPoint(300, 40);
				context.StrokePath();
				context.RestoreState ();
			}
		}
	}

	[Flags]
	public enum Border{
		None = 0,
		Top = 1,
		Right = 2,
		Bottom = 4,
		Left = 8,
		All = Top | Right | Bottom | Left
	}
}

