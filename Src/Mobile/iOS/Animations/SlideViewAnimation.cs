using System;
using System.Drawing;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Animations
{
	public class SlideViewAnimation
	{
		private SizeF _offset;
		private readonly UIView _view;
		private readonly float _duration;
        private Action _action;

        public SlideViewAnimation ( UIView view, SizeF offset, Action action, float duration = 0.5f )
		{
			_view = view;
			_offset = offset;
			_duration = duration;
            _action = action;
		}

		public void Animate()
		{
            UIView.Animate(
                _duration, 
                delegate { 
                    _view.Frame = new RectangleF( _view.Frame.X + _offset.Width, _view.Frame.Y + _offset.Height, _view.Frame.Width, _view.Frame.Height ); 
                }, 
                delegate {
                    _action();
                });
		}
	}
}

