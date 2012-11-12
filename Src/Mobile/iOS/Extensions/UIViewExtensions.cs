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

		public static void SetDimensions(this UIView view, float? width = null, float? height = null )
		{
			view.Frame = new System.Drawing.RectangleF(view.Frame.X, view.Frame.Y, width ?? view.Frame.Width, height ?? view.Frame.Height);
		}
	}
}

