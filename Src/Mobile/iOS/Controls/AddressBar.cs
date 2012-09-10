using System;
using MonoTouch.UIKit;
using System.Drawing;
using apcurium.MK.Common.Extensions;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("AddressBar")]
	public class AddressBar : UIView
	{
		public event EventHandler Ended;
		public event EventHandler Started;
		public event EventHandler EditingChanged;
		public delegate void myhandler( int indexer );
		public event myhandler BarItemClicked;

		public event EventHandler AddressSelected;
		public event EventHandler FindCurrentLocationTouched;
		public event EventHandler Activated;

		private bool _isActive;
		private AddressBarButtons _buttons;


		public AddressBar ()
		{
			Initialize();
		}
		
		public AddressBar (IntPtr handle) : base(  handle )
		{
			Initialize();
		}

		private void Initialize()
		{
			IsActive = false;

			_buttons = new AddressBarButtons( new RectangleF( 10, 5, Frame.Width - 20, Frame.Height - 10 ) );
			this.AddSubview( _buttons );
		}

		public void SetTitle ( string title )
		{
			_buttons.BtnAddress.SetTitle( title );
		}

		public void SetPlaceholder( string placeholder )
		{
			_buttons.BtnAddress.SetPlaceholder( placeholder );
		}

		public void Clear()
		{
			_buttons.BtnAddress.Clear();
		}

		public string Text { 
			get { return _buttons.BtnAddress.Text; }
			set { _buttons.BtnAddress.SetAddress( value ); }
		}

		public bool IsActive { 
			get { return _isActive; }
			set { 
				_isActive = value;
				Highlight( _isActive );
			}
		}

		private void Highlight( bool isActive )
		{
			if( isActive )
			{}
			else
			{}
		}

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);

			var colorSpace = CGColorSpace.CreateDeviceRGB();
			var context = UIGraphics.GetCurrentContext();
			
//			var newGradientColors = AppStyle.GetButtonColors( AppStyle.ButtonColor.Silver ).Select( c => c.CGColor ).ToArray();
//			var newGradientLocations = AppStyle.GetButtonColorLocations( AppStyle.ButtonColor.Silver );
//			var newGradient = new CGGradient(colorSpace, newGradientColors, newGradientLocations);
//
//			var radius = 0;
//
//			ShadowSetting dropShadow = null;
//			var innerShadow = AppStyle.GetInnerShadow( AppStyle.ButtonColor.Silver );
//
//			rect.Width -= dropShadow != null ? Math.Abs(dropShadow.Offset.Width) : 0;
//			rect.Height -= dropShadow != null ? Math.Abs(dropShadow.Offset.Height) : 0;
//			rect.X += dropShadow != null && dropShadow.Offset.Width < 0 ? Math.Abs(dropShadow.Offset.Width) : 0;
//			rect.Y += dropShadow != null && dropShadow.Offset.Height < 0 ? Math.Abs(dropShadow.Offset.Height) : 0;
//
//
//			var roundedRectanglePath = UIBezierPath.FromRoundedRect(rect, radius);
//			if( !ClearBackground )
//			{
//				context.SaveState();
//				if (dropShadow != null)
//				{
//					context.SetShadowWithColor(dropShadow.Offset, dropShadow.BlurRadius, dropShadow.Color.CGColor);
//				}
//				
//				context.BeginTransparencyLayer(null);
//				roundedRectanglePath.AddClip();
//				context.DrawLinearGradient(newGradient, new PointF(rect.X + (rect.Width / 2.0f), rect.Y), new PointF(rect.X + (rect.Width / 2.0f), rect.Y + rect.Height), 0);
//				context.EndTransparencyLayer();
//				context.RestoreState();
//			}
//
//			if (innerShadow != null)
//			{
//				var roundedRectangleBorderRect = roundedRectanglePath.Bounds;
//				roundedRectangleBorderRect.Inflate(innerShadow.BlurRadius, innerShadow.BlurRadius);
//				roundedRectangleBorderRect.Offset(-innerShadow.Offset.Width, -innerShadow.Offset.Height);
//				roundedRectangleBorderRect = RectangleF.Union(roundedRectangleBorderRect, roundedRectanglePath.Bounds);
//				roundedRectangleBorderRect.Inflate(1, 1);
//				
//				var roundedRectangleNegativePath = UIBezierPath.FromRect(roundedRectangleBorderRect);
//				roundedRectangleNegativePath.AppendPath(roundedRectanglePath);
//				roundedRectangleNegativePath.UsesEvenOddFillRule = true;
//				
//				context.SaveState();
//				{
//					var xOffset = innerShadow.Offset.Width + (float)Math.Round(roundedRectangleBorderRect.Width);
//					var yOffset = innerShadow.Offset.Height;
//					context.SetShadowWithColor(
//						new SizeF(xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)),
//						innerShadow.BlurRadius,
//						innerShadow.Color.CGColor);
//					
//					roundedRectanglePath.AddClip();
//					var transform = CGAffineTransform.MakeTranslation(-(float)Math.Round(roundedRectangleBorderRect.Width), 0);
//					roundedRectangleNegativePath.ApplyTransform(transform);
//					UIColor.Gray.SetFill();
//					roundedRectangleNegativePath.Fill();
//				}
//				context.RestoreState();
//			}

			


		}
	}
}

