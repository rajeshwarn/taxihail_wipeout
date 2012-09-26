using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class BarButtonItem : UIButton
	{
		private string _image;
		private Action _onClick;

		public BarButtonItem ( RectangleF rect, string image, Action onClick ) : base( rect )
		{
			_image = image;
			_onClick = onClick;
			Initialize(  );
		}

		private void Initialize()
		{
			BackgroundColor = UIColor.Clear;
			Layer.BorderWidth = 1f;
			Layer.BorderColor = UIColor.FromRGBA(36,44,51,255).CGColor;
			Layer.CornerRadius = AppStyle.ButtonCornerRadius;
			SetImage( UIImage.FromFile( _image ), UIControlState.Normal );
			TouchUpInside += delegate {
				if( _onClick != null )
				{
					_onClick();
				}
			};
		}

	}
}

