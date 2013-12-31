using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Animations
{
	public class SlideViewAnimation
	{
		private SizeF _offset;
		private readonly UIView _view;
		private readonly float _duration;

		public SlideViewAnimation ( UIView view, SizeF offset, float duration = 0.5f )
		{
			_view = view;
			_offset = offset;
			_duration = duration;
		}

		public void Animate()
		{
			UIView.BeginAnimations ( "Slide" );
			UIView.SetAnimationDuration ( _duration );
			_view.Frame = new RectangleF( _view.Frame.X + _offset.Width, _view.Frame.Y + _offset.Height, _view.Frame.Width, _view.Frame.Height );			
			UIView.CommitAnimations ();			
		}
	}
}

