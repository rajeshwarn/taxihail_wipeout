using System;
using System.Drawing;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class VerticalButtonBar : UIView
	{
		public enum AnimationType { Wheel, Arrow };
		public enum AnimationDirection { Down, Up };

		private Dictionary<int, VerticalButton> _buttons;
		public VerticalButton _mainBtn;
		private float _btnHeight = 40f;

		public delegate void ButtonClickedEventHandler(int index);
		public event ButtonClickedEventHandler ButtonClicked;


		public VerticalButtonBar ( RectangleF frame, AnimationType animationType, AnimationDirection animationDirection ) : base ( frame )
		{
			_buttons = new Dictionary<int, VerticalButton>();
			AutosizesSubviews = false;

			BackgroundColor = UIColor.Clear;
			Layer.MasksToBounds = true;
			ContentMode = UIViewContentMode.Top;

			switch( animationType )
			{
			case AnimationType.Wheel:
				_mainBtn = new  AnimatedWheelButton( new RectangleF( 0,0, frame.Width, frame.Height ), UIColor.FromRGB(53,136,204) );
				break;
			case AnimationType.Arrow:
			default:
				_mainBtn = new  AnimatedArrowButton( new RectangleF( 0,0, frame.Width, frame.Height ), UIColor.FromRGB(212,212,212), UIColor.FromRGB(184,184,184));
				break;
			}


			_mainBtn.Direction = animationDirection;
			_mainBtn.FirstButton = true;

			_mainBtn.TouchUpInside -= delegate { AnimateBar(); };
			_mainBtn.TouchUpInside += delegate { AnimateBar(); };

			AddSubview( (VerticalButton)_mainBtn );
			IsOpen = false;
		}

		private bool IsClosing { get; set; }

		private void AnimateBar()
		{
			_mainBtn.Animate( IsOpen, FullHeight );

			UIView.BeginAnimations("Extend");
			UIView.SetAnimationDuration(0.3);
			UIView.SetAnimationCurve( UIViewAnimationCurve.EaseIn );
			if( IsOpen )
			{
				if( _mainBtn.Direction == VerticalButtonBar.AnimationDirection.Down )
				{
					Frame = new RectangleF( Frame.X, Frame.Y, Frame.Width, _mainBtn.Frame.Height );
				}
				else
				{
					Frame = new RectangleF( Frame.X, Frame.Y + FullHeight - _mainBtn.Frame.Height, Frame.Width, _mainBtn.Frame.Height );
					_mainBtn.Frame = new RectangleF( _mainBtn.Frame.X, _mainBtn.Frame.Y - FullHeight + _mainBtn.Frame.Height, _mainBtn.Frame.Width, _mainBtn.Frame.Height );
					foreach( var btn in _buttons.Values )
					{
						btn.Frame = new RectangleF( btn.Frame.X, btn.Frame.Y - FullHeight + _mainBtn.Frame.Height, btn.Frame.Width, btn.Frame.Height );
					}
				}
			}
			else
			{
				if( _mainBtn.Direction == VerticalButtonBar.AnimationDirection.Down)
				{
					Frame = new RectangleF( Frame.X, Frame.Y, Frame.Width, FullHeight );
				}
				else
				{
					Frame = new RectangleF( Frame.X, Frame.Y - FullHeight + _mainBtn.Frame.Height, Frame.Width, FullHeight );
					_mainBtn.Frame = new RectangleF( _mainBtn.Frame.X, _mainBtn.Frame.Y + FullHeight - _mainBtn.Frame.Height, _mainBtn.Frame.Width, _mainBtn.Frame.Height );
					foreach( var btn in _buttons.Values )
					{
						btn.Frame = new RectangleF( btn.Frame.X, btn.Frame.Y + FullHeight - _mainBtn.Frame.Height, btn.Frame.Width, btn.Frame.Height );
					}
				}
			}
			UIView.CommitAnimations();

			IsOpen = !IsOpen;

			_mainBtn.Selected = IsOpen;
		}

		private float FullHeight { get { return _mainBtn.Frame.Height + (_buttons.Count * _btnHeight); } }

		public void AddButton( UIImage normalImage, UIImage selectedImage)
		{
			var lastBtn = _buttons.Count == 0 ? _mainBtn : _buttons[_buttons.Count - 1];
			VerticalButton button = null;
			switch( _mainBtn.Direction )
			{
			case AnimationDirection.Down:
				button = new VerticalButton( new RectangleF(0 ,lastBtn.Frame.Bottom, Frame.Width, _btnHeight), UIColor.FromRGB(242,242,242) );
				break;
			case AnimationDirection.Up:
				button = new VerticalButton( new RectangleF(0 ,lastBtn.Frame.Top - _btnHeight, Frame.Width, _btnHeight), UIColor.FromRGB(242,242,242) );
				break;
			}
			button.AutoresizingMask = UIViewAutoresizing.None;
			button.Direction = _mainBtn.Direction;
			button.ContentMode = UIViewContentMode.Center;
			button.LastButton = true;
			button.SetImage( normalImage, UIControlState.Normal );
			button.SetImage( selectedImage, UIControlState.Selected );
			button.TouchUpInside -= HandleTouchUpInside;
			button.TouchUpInside += HandleTouchUpInside;
			InsertSubview( button, 0 );
			_buttons.Values.Where( b => b.LastButton == true ).ForEach( bb => bb.LastButton = false );
			_buttons.Add( _buttons.Count, button );
		}

		public RectangleF OpenRect {
			get { return new RectangleF( Frame.X, Frame.Y - FullHeight + _mainBtn.Frame.Height, Frame.Width, FullHeight ); }
		}

		void HandleTouchUpInside (object sender, EventArgs e)
		{
			if( ButtonClicked != null )
			{
				var btn = sender as VerticalButton;
				ButtonClicked( _buttons.Single( b => b.Value.Equals( btn ) ).Key );
			}
			AnimateBar();
		}

		public bool IsOpen { get; set; }


//		public override void Draw (RectangleF rect)
//		{
//			//base.Draw (rect);
//
//			var colorSpace = CGColorSpace.CreateDeviceRGB();
//			var context = UIGraphics.GetCurrentContext();
//
//			UIBezierPath rectanglePath;
//			if( IsOpen || IsClosing )
//			{
//				rectanglePath = UIBezierPath.FromRoundedRect(new RectangleF(rect.X,rect.Y, rect.Width, FullHeight ),4f);
//			}
//			else
//			{
//				rectanglePath = UIBezierPath.FromRoundedRect(rect, 4);
//			}
//
//			var firstBtnPath = UIBezierPath.FromRoundedRect( rect, UIRectCorner.TopLeft | UIRectCorner.TopRight, new SizeF(4,4) );
//			if( IsOpen )
//			{
//				UIColor.FromRGB(53,136,204).SetFill();
//				firstBtnPath.Fill();
//				var path = new UIBezierPath();
//				path.MoveTo( new PointF( rect.X + 1, _arrowBtn.Frame.Bottom ) );
//				path.AddLineTo( new PointF( rect.Right - 1, _arrowBtn.Frame.Bottom ) );
//				UIColor.FromRGB(36,44,51).SetStroke();
//				path.LineWidth = 1;
//				path.Stroke();
//			}
//			else
//			{
//				CGColor[] newGradientColors;
//				newGradientColors = new CGColor [] {UIColor.FromRGB(212,212,212).CGColor, UIColor.FromRGB(184,184,184).CGColor};
//				var newGradientLocations = new float [] {0, 1};
//				var newGradient = new CGGradient(colorSpace, newGradientColors, newGradientLocations);
//
//				context.SaveState();
//				firstBtnPath.AddClip();
//				context.DrawLinearGradient(newGradient, new PointF(( rect.Width / 2 ) + rect.X, rect.Y), new PointF((rect.Width/ 2 ) + rect.X, rect.Y + rect.Height), 0);
//				context.RestoreState();
//			}
//
//			//// Shadow Declarations
//			var shadow2 = UIColor.White.ColorWithAlpha(0.5f).CGColor;
//			var shadow2Offset = new SizeF(0, 1);
//			var shadow2BlurRadius = 0;
//			////// Rounded Rectangle Inner Shadow
//			var roundedRectangleBorderRect = new RectangleF( rect.X, rect.Y+1, rect.Width, rect.Height - 1 ); // rectanglePath.Bounds;
//			roundedRectangleBorderRect.Inflate(shadow2BlurRadius, shadow2BlurRadius);
//			roundedRectangleBorderRect.Offset(-shadow2Offset.Width, -shadow2Offset.Height);
//			roundedRectangleBorderRect = RectangleF.Union(roundedRectangleBorderRect, new RectangleF( rect.X, rect.Y+1, rect.Width, rect.Height - 1 ));
//			roundedRectangleBorderRect.Inflate(1, 1);
//
//			var roundedRectangleNegativePath = UIBezierPath.FromRect(roundedRectangleBorderRect);
//			var rectanglePath2 = UIBezierPath.FromRoundedRect(new RectangleF( rect.X, rect.Y+1, rect.Width, rect.Height - 1 ), UIRectCorner.TopLeft | UIRectCorner.TopRight, new SizeF(4, 4));
//			roundedRectangleNegativePath.AppendPath(rectanglePath2);
//			roundedRectangleNegativePath.UsesEvenOddFillRule = true;
//
//			context.SaveState();
//			{
//			    var xOffset = shadow2Offset.Width + (float)Math.Round(roundedRectangleBorderRect.Width);
//			    var yOffset = shadow2Offset.Height;
//			    context.SetShadowWithColor(
//			        new SizeF(xOffset + (xOffset >= 0 ? 0.1f : -0.1f), yOffset + (yOffset >= 0 ? 0.1f : -0.1f)),
//			        shadow2BlurRadius,
//			        shadow2);
//
//			    rectanglePath2.AddClip();
//			    var transform = CGAffineTransform.MakeTranslation(-(float)Math.Round(roundedRectangleBorderRect.Width), 0);
//			    roundedRectangleNegativePath.ApplyTransform(transform);
//			    UIColor.Gray.SetFill();
//			    roundedRectangleNegativePath.Fill();
//			}
//			context.RestoreState();
//
//			var dropdownPath = UIBezierPath.FromRoundedRect( new RectangleF( rect.X, _arrowBtn.Frame.Bottom, rect.Width, _buttons.Count * _btnHeight ), UIRectCorner.BottomLeft | UIRectCorner.BottomRight, new SizeF(4,4) );
//			UIColor.FromRGB(242,242,242).SetFill();
//			dropdownPath.Fill();
//
//			for( int i = 1; i <= _buttons.Count - 1; i++ )
//			{
//				var height = _arrowBtn.Frame.Height + i*_btnHeight;
//
//				var path = new UIBezierPath();
//				path.MoveTo( new PointF( rect.X + 1, height ) );
//				path.AddLineTo( new PointF( rect.Right - 1, height ) );
//				UIColor.FromRGB(207,211,214).SetStroke();
//				path.LineWidth = 1;
//				path.Stroke();
//
//				path = new UIBezierPath();
//				path.MoveTo( new PointF( rect.X + 1, height +1 ) );
//				path.AddLineTo( new PointF( rect.Right - 1, height +1 ) );
//				UIColor.FromRGB(253,253,253).SetStroke();
//				path.LineWidth = 1;
//				path.Stroke();
//			}
//
//			//Overall stroke
//			if( IsOpen )
//			{
//				UIColor.FromRGB(36,44,51).SetStroke();
//			}
//			else
//			{
//				UIColor.FromRGB(127,127,127).SetStroke();
//			}
//			rectanglePath.LineWidth = 1;
//			rectanglePath.Stroke();
//
//		}
	}
}

