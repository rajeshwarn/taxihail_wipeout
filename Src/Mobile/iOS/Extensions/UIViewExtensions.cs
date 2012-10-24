using System;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client
{
	public static class UIViewExtensions
	{
		public static void SetPosition(this UIView view, float? x = null, float? y = null )
		{
			view.Frame = new System.Drawing.RectangleF(x ?? view.Frame.X, y ?? view.Frame.Y, view.Frame.Width, view.Frame.Height);
		}
	}
}

