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

        public static UIView FindFirstResponder (this UIView view)
        {
            if (view.IsFirstResponder)
            {
                return view;
            }
            foreach (UIView subView in view.Subviews) {
                var firstResponder = subView.FindFirstResponder();
                if (firstResponder != null)
                    return firstResponder;
            }
            return null;
        }
        
        public static UIView FindSuperviewOfType(this UIView view, UIView stopAt, Type type)
        {
            if (view.Superview != null)
            {
                if (type.IsAssignableFrom(view.Superview.GetType()))
                {
                    return view.Superview;
                }
                
                if (view.Superview != stopAt)
                    return view.Superview.FindSuperviewOfType(stopAt, type);
            }
            
            return null;
        }

        public static T FindSuperviewOfType<T>(this UIView view, UIView stopAt) where T:UIView
        {
            if (view.Superview != null)
            {
                if (view.Superview is T)
                {
                    return (T)view.Superview;
                }
                
                if (view.Superview != stopAt)
                    return view.Superview.FindSuperviewOfType<T>(stopAt);
            }
            
            return null;
        }
	}
}

