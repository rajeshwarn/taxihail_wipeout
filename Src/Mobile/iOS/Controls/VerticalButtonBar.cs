using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public sealed class VerticalButtonBar : UIView
	{
		public enum AnimationType { Wheel };
		public enum AnimationDirection { Down, Up };

		private readonly Dictionary<int, VerticalButton> _buttons;
		public VerticalButton _mainBtn;
		private float _btnHeight = 40f;

		public delegate void ButtonClickedEventHandler(int index);


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
			    default:
				_mainBtn = new  AnimatedArrowButton( new RectangleF( 0,0, frame.Width, frame.Height ), UIColor.FromRGB(212,212,212), UIColor.FromRGB(184,184,184));
				break;
			}


			_mainBtn.Direction = animationDirection;
			_mainBtn.FirstButton = true;

		    _mainBtn.TouchUpInside -= OnMainBtnOnTouchUpInside;
            _mainBtn.TouchUpInside += OnMainBtnOnTouchUpInside;

			AddSubview( _mainBtn );
			IsOpen = false;
		}

	    private void OnMainBtnOnTouchUpInside(object sender, EventArgs e)
	    {
	        AnimateBar();
	    }

	    private void AnimateBar()
		{
			_mainBtn.Animate( IsOpen, FullHeight );

			BeginAnimations("Extend");
			SetAnimationDuration(0.3);
			SetAnimationCurve( UIViewAnimationCurve.EaseIn );
			if( IsOpen )
			{
				if( _mainBtn.Direction == AnimationDirection.Down )
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
				if( _mainBtn.Direction == AnimationDirection.Down)
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
			CommitAnimations();

			IsOpen = !IsOpen;

			_mainBtn.Selected = IsOpen;
		}

		private float FullHeight { get { return _mainBtn.Frame.Height + (_buttons.Count * _btnHeight); } }

	    public bool IsOpen { get; set; }
	}
}

