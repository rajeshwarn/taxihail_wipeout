using System.Drawing;
using System.Linq;
using apcurium.MK.Common.Extensions;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class ToastMessage : UIView
	{
		private readonly string _msg;
		private readonly UIView _owner;

		private float _sidePadding = 10f;
		private float _interiorPadding = 5f;
		private float _bottomPadding = 80f;

	    private int _toastDuration = 2; //seconds

		private double _animationDuration = 2;
		private float _minimumToastHeight = 30f;


		public ToastMessage ( UIView owner, string msg )
		{
			_msg = msg;
			_owner = owner;
			Initialize();
		}

		private void Initialize()
		{
			BackgroundColor = UIColor.FromRGBA( 0, 0, 0, 100 );
            Layer.CornerRadius = 6; //AppStyle.ButtonCornerRadius;
			Layer.BorderWidth = 1;
			Layer.BorderColor = UIColor.FromRGBA(0,0,0,110).CGColor;

			var screenSize = UIScreen.MainScreen.Bounds;
			var textSize = ((NSString)_msg).StringSize(AppStyle.NormalTextFont, screenSize.Width - ((_sidePadding + _interiorPadding) *2), UILineBreakMode.TailTruncation );

			Frame = new RectangleF( _sidePadding, screenSize.Height - _bottomPadding - (textSize.Height > _minimumToastHeight ? textSize.Height : _minimumToastHeight), screenSize.Width - (_sidePadding * 2), (textSize.Height > _minimumToastHeight ? textSize.Height : _minimumToastHeight) );

			var lblMsg = new UILabel( new RectangleF( _interiorPadding, _interiorPadding, Frame.Width - (_interiorPadding * 2), Frame.Height - (_interiorPadding * 2) ) );
			lblMsg.BackgroundColor = UIColor.Clear;
			lblMsg.TextColor = UIColor.FromRGBA( 255,255,255,255);
			lblMsg.TextAlignment = UITextAlignment.Center;
			lblMsg.Text = _msg;
            lblMsg.Font = AppStyle.BoldTextFont;
			AddSubview( lblMsg );

		}

		public void Show( int duration )
		{
			_owner.Subviews.Where( v => v is ToastMessage ).ForEach( vv => vv.RemoveFromSuperview() );
			Animate( _animationDuration, () => _owner.AddSubview( this ) );
			NSTimer.CreateScheduledTimer( duration == 0 ? _toastDuration : duration, () => Hide() );
		}

		private void Hide()
		{
			Animate( _animationDuration, () => RemoveFromSuperview() );
		}
	}
}

